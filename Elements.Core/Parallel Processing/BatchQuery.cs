using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elements.Core
{
    public abstract class BatchQuery<Query, Result>
        where Query : class, IEquatable<Query>
        where Result : class
    {
        object _lock = new object();

        protected class QueryResult
        {
            public readonly Query query;
            public Result result;

            public QueryResult(Query query)
            {
                this.query = query;
            }
        }

        public int MaxBatchSize { get; set; } = 32;
        public float DelaySeconds { get; set; } = 0.25f;

        public BatchQuery(int maxBatchSize = 32, float delaySeconds = 0.25f)
        {
            this.MaxBatchSize = maxBatchSize;
            this.DelaySeconds = delaySeconds;
        }

        Dictionary<Query, TaskCompletionSource<Result>> queue = new Dictionary<Query, TaskCompletionSource<Result>>();
        TaskCompletionSource<bool> immediateDispatch;
        volatile bool dispatchScheduled;

        public async Task<Result> Request(Query query)
        {
            TaskCompletionSource<Result> task = null;

            lock(_lock)
            {
                if(!queue.TryGetValue(query, out task))
                {
                    task = new TaskCompletionSource<Result>();
                    queue.Add(query, task);

                    // start the dispatch
                    if (!dispatchScheduled)
                    {
                        dispatchScheduled = true;
                        immediateDispatch = new TaskCompletionSource<bool>();
                        Task.Run(SendBatch);
                    }
                    else if (queue.Count >= MaxBatchSize)
                        immediateDispatch.TrySetResult(true);
                }
            }

            return await task.Task;
        }

        async Task SendBatch()
        {
            await Task.WhenAny(immediateDispatch.Task, Task.Delay(TimeSpan.FromSeconds(DelaySeconds)));

            var toSend = Pool.BorrowList<QueryResult>();

            // fetch queries to send
            lock (_lock)
            {
                foreach(var query in queue)
                {
                    toSend.Add(new QueryResult(query.Key));

                    if (toSend.Count == MaxBatchSize)
                        break;
                }
            }

            if (toSend.Count > 0)
                await RunBatch(toSend);

            // distribute the finished queries
            lock(_lock)
            {
                foreach(var queryResult in toSend)
                {
                    queue[queryResult.query].SetResult(queryResult.result);
                    queue.Remove(queryResult.query);
                }

                // check if it should be dispatched again
                if (queue.Count > 0)
                {
                    if (queue.Count >= MaxBatchSize)
                        immediateDispatch.TrySetResult(true);
                    else
                        immediateDispatch = new TaskCompletionSource<bool>();

                    Task.Run(SendBatch);
                }
                else
                    dispatchScheduled = false;
            }
        }

        protected abstract Task RunBatch(List<QueryResult> batch);
    }
}
