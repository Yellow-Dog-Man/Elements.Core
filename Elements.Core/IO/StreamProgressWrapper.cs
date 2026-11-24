using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace Elements.Core
{
    public class StreamProgressWrapper : System.IO.Stream, IDisposable
    {
        System.IO.Stream innerStream;
        IProgressIndicator indicator;
        Action<System.IO.Stream, IProgressIndicator> updateActionOverride;
        long? overrideTargetLength;

        public StreamProgressWrapper(System.IO.Stream stream, IProgressIndicator indicator,
            Action<System.IO.Stream, IProgressIndicator> updateActionOverride = null,
            long? overrideTargetLength = null)
        {
            this.innerStream = stream;
            this.indicator = indicator;
            this.updateActionOverride = updateActionOverride;
            this.overrideTargetLength = overrideTargetLength;
        }

        public override bool CanRead => innerStream.CanRead;

        public override bool CanSeek => innerStream.CanSeek;

        public override bool CanWrite => innerStream.CanWrite;

        public override long Length => innerStream.Length;

        public override long Position
        {
            get => innerStream.Position;
            set
            {
                innerStream.Position = value;
                UpdateProgress();
            }
        }

        public override bool CanTimeout => innerStream.CanTimeout;
        public override int ReadTimeout { get => innerStream.ReadTimeout; set => innerStream.ReadTimeout = value; }
        public override int WriteTimeout { get => innerStream.WriteTimeout; set => innerStream.WriteTimeout = value; }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return innerStream.BeginRead(buffer, offset, count, callback, state);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return base.BeginWrite(buffer, offset, count, callback, state);
        }

        public override void Close()
        {
            innerStream.Close();
            base.Close();
        }

        // DO NOT OVERRIDE THIS ONE, this breaks the UpdateProgress as it yields all control to the inner stream
        //public override System.Threading.Tasks.Task CopyToAsync(System.IO.Stream destination, int bufferSize, CancellationToken cancellationToken)
        //{
        //    UniLog.Log("CopyToAsync");

        //    return innerStream.CopyToAsync(destination, bufferSize, cancellationToken);
        //}

        public override int EndRead(IAsyncResult asyncResult)
        {
            var result = innerStream.EndRead(asyncResult);
            UpdateProgress();
            return result;
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            innerStream.EndWrite(asyncResult);
            UpdateProgress();
        }

        public override System.Threading.Tasks.Task FlushAsync(CancellationToken cancellationToken) => innerStream.FlushAsync(cancellationToken);

        public override async System.Threading.Tasks.Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            var result = await innerStream.ReadAsync(buffer, offset, count, cancellationToken);
            UpdateProgress();
            return result;
        }

        public override int ReadByte()
        {
            var result = innerStream.ReadByte();
            UpdateProgress();
            return result;
        }

        public override async System.Threading.Tasks.Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            await base.WriteAsync(buffer, offset, count, cancellationToken);
            UpdateProgress();
        }

        public override void WriteByte(byte value)
        {
            base.WriteByte(value);
            UpdateProgress();
        }

        public override void Flush() => innerStream.Flush();

        public override int Read(byte[] buffer, int offset, int count)
        {
            var read = innerStream.Read(buffer, offset, count);
            UpdateProgress();
            return read;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            var result = innerStream.Seek(offset, origin);
            UpdateProgress();
            return result;
        }

        public override void SetLength(long value)
        {
            innerStream.SetLength(value);
            UpdateProgress();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            innerStream.Write(buffer, offset, count);
            UpdateProgress();
        }

        protected override void Dispose(bool disposing)
        {
            innerStream.Dispose();
            base.Dispose(disposing);
        }

        void UpdateProgress()
        {
            if (indicator != null)
            {
                if (updateActionOverride != null)
                    updateActionOverride(innerStream, indicator);
                else
                {
                    float percent = Position / (float)(overrideTargetLength ?? Length);
                    indicator.UpdateProgress(percent, Elements.Core.UnitFormatting.FormatBytes(Position), "");
                }
            }
        }
    }
}
