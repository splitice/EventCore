using System.Diagnostics.Contracts;
using System.Net;
using EventCore.Memory;
using EventCore.Memory.Buffer;
using EventCore.Networking.Sockets;

namespace EventCore.Networking.Event
{
    public delegate void ReadFunction(IByteBuffer buffer, EndPoint from);

    [ContractClass(typeof (ContractIEventHelpers))]
    internal interface IEventHelpers
    {
        void RegisterSocketRead(IEvent e, BufferedSocket socket, IEventHandler protocol, ReadFunction callback);
        void RegisterSocketPeekRead(IEvent e, BufferedSocket socket, IEventHandler protocol, ReadFunction callback);
    }

    [ContractClassFor(typeof (IEventHelpers))]
    internal abstract class ContractIEventHelpers : IEventHelpers
    {
        #region IEventHelpers Members

        void IEventHelpers.RegisterSocketRead(IEvent e, BufferedSocket socket, IEventHandler protocol,
                                              ReadFunction callback)
        {
            //Contract.Requires(socket != null);
            Contract.Requires(protocol != null);
            Contract.Requires(callback != null);
        }

        void IEventHelpers.RegisterSocketPeekRead(IEvent e, BufferedSocket socket, IEventHandler protocol,
                                                  ReadFunction callback)
        {
            //Contract.Requires(socket != null);
            Contract.Requires(protocol != null);
            Contract.Requires(callback != null);
        }

        #endregion
    }
}