using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using EventCore.Networking.Sockets;
using EventCore.Networking.Timer;

namespace EventCore.Networking.Event
{
    [ContractClass(typeof (ContractIEvent))]
    public interface IEvent
    {
        EventTimer Timer { get; }
        DateTime StartOfRun { get; }
        void RegisterSocketRead(BufferedSocket socket, Action callback);
        void RegisterSocketWrite(BufferedSocket socket, Action callback);
        void RegisterSocketAccept(BufferedSocket socket, Action<BufferedSocket> callback);
        void UpdateSocketAccept(BufferedSocket socket, Action<BufferedSocket> callback);
        void UnregisterSocketRead(BufferedSocket socket);
        void UnregisterSocketWrite(BufferedSocket socket);
        void UnregisterSocketAccept(BufferedSocket socket);
        void UnregisterSocket(BufferedSocket socket);
        void Run();
    }

    [ContractClassFor(typeof (IEvent))]
    internal abstract class ContractIEvent : IEvent
    {
        public List<ISocket> Sockets { get; set; }

        #region IEvent Members

        public EventTimer Timer { get; private set; }
        public DateTime StartOfRun { get; private set; }

        void IEvent.RegisterSocketRead(BufferedSocket socket, Action callback)
        {
            //Contract.Requires(socket != null);
            Contract.Requires(callback != null);
        }

        void IEvent.RegisterSocketWrite(BufferedSocket socket, Action callback)
        {
            //Contract.Requires(socket != null);
            Contract.Requires(callback != null);
        }

        void IEvent.RegisterSocketAccept(BufferedSocket socket, Action<BufferedSocket> callback)
        {
            //Contract.Requires(socket != null);
            Contract.Requires(callback != null);
        }

        void IEvent.UpdateSocketAccept(BufferedSocket socket, Action<BufferedSocket> callback)
        {
            //Contract.Requires(socket != null);
            Contract.Requires(callback != null);
        }

        void IEvent.UnregisterSocketRead(BufferedSocket socket)
        {
            //Contract.Requires(socket != null);
        }

        void IEvent.UnregisterSocketWrite(BufferedSocket socket)
        {
            //Contract.Requires(socket != null);
        }

        void IEvent.UnregisterSocketAccept(BufferedSocket socket)
        {
            //Contract.Requires(socket != null);
        }

        void IEvent.UnregisterSocket(BufferedSocket socket)
        {
            //Contract.Requires(socket != null);
        }

        void IEvent.Run()
        {
        }

        #endregion
    }
}