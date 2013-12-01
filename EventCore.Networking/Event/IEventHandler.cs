using System.Diagnostics.Contracts;
using System.Net;
using EventCore.Memory;
using EventCore.Memory.Buffer;
using EventCore.Networking.Sockets;

namespace EventCore.Networking.Event
{
    [ContractClass(typeof (IEventHandler))]
    public interface IEventHandler
    {
        int Read(BufferedSocket wrappedSocket, IByteBuffer msg, ref EndPoint ep);
        int Peek(BufferedSocket bufferedSocket, IByteBuffer msg, ref EndPoint ep);
    }

    [ContractClassFor(typeof (IEventHandler))]
    internal class ContractIEventHandler : IEventHandler
    {
        #region IEventHandler Members

        int IEventHandler.Read(BufferedSocket socket, IByteBuffer msg, ref EndPoint ep)
        {
            //Contract.Requires(socket != null);
            Contract.Requires(msg != null);
            Contract.Requires(ep != null);

            return default(int);
        }

        int IEventHandler.Peek(BufferedSocket bufferedSocket, IByteBuffer msg, ref EndPoint ep)
        {
            Contract.Requires(ep != null);
            //Contract.Requires(bufferedSocket != null);

            return default(int);
        }

        #endregion
    }
}