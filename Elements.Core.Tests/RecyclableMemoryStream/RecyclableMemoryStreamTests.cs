using System.IO.Pipes;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Elements.Core.Tests;

[TestClass]
public class RecyclableMemoryStreamTests
{
    [TestMethod]
    public async Task WriteFromPartialBuffer()
    {
        await using var pipeServer = new AnonymousPipeServerStream(PipeDirection.Out);

        pipeServer.Write(new byte[]{1, 2, 3, 4, 5});
        
        var handle = pipeServer.ClientSafePipeHandle;
        var child = Task.Run(async () =>
        {
            await using var pipeClient = new AnonymousPipeClientStream(PipeDirection.In, handle);
            var manager = new RecyclableMemoryStreamManager();
            await using var recycleStream = new RecyclableMemoryStream(manager, null , 100);

            recycleStream.WriteFrom(pipeClient, 10);
            return recycleStream.ToArray();
        });
        
        await Task.Delay(10);
        pipeServer.Write(new byte[]{6, 7, 8, 9, 10});
        var memory = await child;
        
        CollectionAssert.AreEqual(new byte[]{1, 2, 3, 4, 5, 6, 7, 8, 9, 10}, memory);
    }
}
