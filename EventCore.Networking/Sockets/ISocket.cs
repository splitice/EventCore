using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Sockets;

namespace EventCore.Networking.Sockets
{
    [ContractClass(typeof (ContractISocket))]
    public interface ISocket
    {
        IntPtr Handle { get; }
        Socket Socket { get; }
        ProtocolType ProtocolType { get; }
        bool Connected { get; }
        EndPoint LocalEndPoint { get; }
        EndPoint RemoteEndPoint { get; }
        bool IsBound { get; }
        int OldHandle { get; }
        void Bind(IPEndPoint from);
        void Bind(IPAddress from);
        bool Poll(SelectMode mode);
        bool Connect(IPEndPoint to);
        int Send(IList<ArraySegment<byte>> message);
        int Send(IList<ArraySegment<byte>> message, int length);
        int Send(IList<ArraySegment<byte>> message, int start, int length);
        int Send(byte[] message, int length);
        int Send(byte[] message, int start, int length);
        int SendTo(byte[] message, int length, IPEndPoint to);
        int SendTo(byte[] message, int length, EndPoint to);
        int Receive(byte[] message);
        int Receive(byte[] message, int length);
        int Receive(byte[] message, int position, int length);
        int ReceiveFrom(byte[] message, ref IPEndPoint from);
        int ReceiveFrom(byte[] message, ref EndPoint from);
        int Peek(byte[] message);
        bool Close();
        void Listen(int backlog);
        ISocket Accept();
        void Shutdown(SocketShutdown shutdown);
        void SetSocketOption(SocketOptionLevel level, SocketOptionName name, Object value);
        int Peek(byte[] message, int offset, int count);
        int ReceiveFrom(byte[] message, int offset, int count, ref EndPoint to);
        int SendTo(byte[] message, int offset, int count, IPEndPoint to);
        int SendTo(byte[] message, int offset, int count, EndPoint to);
    }

    [ContractClassFor(typeof (ISocket))]
    internal abstract class ContractISocket : ISocket
    {
        #region ISocket Members

        public int OldHandle { get; private set; }

        public int Peek(byte[] message, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public int ReceiveFrom(byte[] message, int offset, int count, ref EndPoint to)
        {
            throw new NotImplementedException();
        }

        public int SendTo(byte[] message, int offset, int count, IPEndPoint to)
        {
            throw new NotImplementedException();
        }

        public int SendTo(byte[] message, int offset, int count, EndPoint to)
        {
            throw new NotImplementedException();
        }

        public Socket Socket { get; private set; }
        public IntPtr Handle { get; private set; }
        public ProtocolType ProtocolType { get; private set; }

        void ISocket.Shutdown(SocketShutdown shutdown)
        {
        }

        public void SetSocketOption(SocketOptionLevel level, SocketOptionName name, object value)
        {
            //throw new NotImplementedException();
        }

        void ISocket.Bind(IPEndPoint from)
        {
            Contract.Requires(from != null);
        }

        void ISocket.Bind(IPAddress from)
        {
            Contract.Requires(from != null);
        }

        bool ISocket.Connect(IPEndPoint to)
        {
            Contract.Requires(to != null);
            return default(bool);
        }

        public bool Connected { get; private set; }

        bool ISocket.Poll(SelectMode mode)
        {
            return default(bool);
        }

        int ISocket.Send(IList<ArraySegment<byte>> message)
        {
            return default(int);
        }

        int ISocket.Send(IList<ArraySegment<byte>> message, int length)
        {
            return default(int);
        }

        int ISocket.Send(IList<ArraySegment<byte>> message, int start, int length)
        {
            return default(int);
        }

        int ISocket.Send(byte[] message, int length)
        {
            Contract.Requires(message != null);
            Contract.Requires(length > 0);

            return default(int);
        }

        int ISocket.Send(byte[] message, int start, int length)
        {
            Contract.Requires(message != null);
            Contract.Requires(length > 0);
            Contract.Requires(start >= 0);

            return default(int);
        }

        int ISocket.SendTo(byte[] message, int length, IPEndPoint to)
        {
            Contract.Requires(message != null);
            Contract.Requires(to != null);

            return default(int);
        }

        int ISocket.SendTo(byte[] message, int length, EndPoint to)
        {
            Contract.Requires(message != null);
            Contract.Requires(to != null);

            return default(int);
        }

        int ISocket.Receive(byte[] message)
        {
            Contract.Requires(message != null);

            return default(int);
        }

        int ISocket.Receive(byte[] message, int length)
        {
            Contract.Requires(message != null);
            Contract.Requires(length > 0);

            return default(int);
        }

        public int Receive(byte[] message, int position, int length)
        {
            throw new NotImplementedException();
        }

        int ISocket.ReceiveFrom(byte[] message, ref EndPoint from)
        {
            Contract.Requires(message != null);
            Contract.Ensures(Contract.ValueAtReturn(out from) != null);

            from = default(EndPoint);

            return default(int);
        }

        int ISocket.ReceiveFrom(byte[] message, ref IPEndPoint from)
        {
            Contract.Requires(message != null);
            Contract.Ensures(Contract.ValueAtReturn(out from) != null);

            from = default(IPEndPoint);

            return default(int);
        }

        int ISocket.Peek(byte[] message)
        {
            Contract.Requires(message != null);

            return default(int);
        }

        bool ISocket.Close()
        {
            return default(bool);
        }

        void ISocket.Listen(int backlog)
        {
            Contract.Requires(backlog > 0);
        }

        public EndPoint LocalEndPoint { get; private set; }
        public EndPoint RemoteEndPoint { get; private set; }
        public bool IsBound { get; private set; }

        ISocket ISocket.Accept()
        {
            return default(ISocket);
        }

        #endregion
    }
}