using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace EventCore.Networking.Sockets
{
    public class WrappedSocket : ISocket
    {
        private readonly Socket _socket;

        public WrappedSocket(Socket socket)
        {
            //Contract.Requires(socket != null);

            _socket = socket;
            _socket.Blocking = false;
            OldHandle = -1;
        }

        public WrappedSocket(AddressFamily family, SocketType socketType, ProtocolType protocolType)
        {
            _socket = new Socket(family, socketType, protocolType) {Blocking = false};
            OldHandle = -1;
        }

        #region ISocket Members

        public void Shutdown(SocketShutdown shutdown)
        {
            _socket.Shutdown(shutdown);
        }

        public Socket Socket
        {
            get { return _socket; }
        }

        public ProtocolType ProtocolType
        {
            get { return _socket.ProtocolType; }
        }

        public IntPtr Handle
        {
            get { return _socket.Handle; }
        }

        public int OldHandle { get; private set; }

        public int Peek(byte[] message, int offset, int count)
        {
            return _socket.Receive(message, offset, count, SocketFlags.Peek);
        }

        public int ReceiveFrom(byte[] message, int offset, int count, ref EndPoint to)
        {
            return _socket.ReceiveFrom(message, offset, count, SocketFlags.None, ref to);
        }

        public int SendTo(byte[] message, int offset, int count, IPEndPoint to)
        {
            return _socket.SendTo(message, offset, count, SocketFlags.None, to);
        }

        public int SendTo(byte[] message, int offset, int count, EndPoint to)
        {
            return _socket.SendTo(message, offset, count, SocketFlags.None, to);
        }

        public void Bind(IPEndPoint from)
        {
            try
            {
                _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                _socket.Bind(from);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public void Bind(IPAddress from)
        {
            Bind(new IPEndPoint(from, 0));
        }

        public bool Connect(IPEndPoint to)
        {
            try
            {
                _socket.Connect(to);
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode == SocketError.WouldBlock)
                {
                    return false;
                }
            }

            return true;
        }

        public bool Connected
        {
            get { return _socket.Connected; }
        }

        public bool Poll(SelectMode mode)
        {
            return _socket.Poll(0, mode);
        }

        public int Send(byte[] message, int length)
        {
            return Send(message, 0, length);
        }

        public int Send(byte[] message, int start, int length)
        {
            try
            {
                return _socket.Send(message, start, length, SocketFlags.None);
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode == SocketError.WouldBlock)
                {
                    return 0;
                }

                throw;
            }
        }

        public int Send(IList<ArraySegment<byte>> message)
        {
            return _socket.Send(message);
        }

        public int Send(IList<ArraySegment<byte>> message, int length)
        {
            return Send(message, 0, length);
        }

        public int Send(IList<ArraySegment<byte>> message, int start, int length)
        {
            try
            {
                IList<ArraySegment<byte>> toSend = new List<ArraySegment<byte>>(message.Count);
                int count = 0, end = start + length;
                var last = new ArraySegment<byte>();
                foreach (var segment in message)
                {
                    if (count > start)
                    {
                        toSend.Add(last);
                    }
                    last = segment;
                    count += segment.Count;
                    length -= segment.Count;
                    if (length != -1 && count >= end)
                    {
                        if (count != end)
                        {
                            ArraySegment<byte> sTemp = toSend[toSend.Count - 1];
                            toSend[toSend.Count - 1] = new ArraySegment<byte>(sTemp.Array, sTemp.Offset,
                                                                              sTemp.Count + length);
                        }
                        break;
                    }
                }
                //In this case we need to manage 
                return _socket.Send(message);
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode == SocketError.WouldBlock)
                {
                    return 0;
                }

                throw;
            }
        }

        public int SendTo(byte[] message, int length, IPEndPoint to)
        {
            return SendTo(message, length, (EndPoint) to);
        }

        public int SendTo(byte[] message, int length, EndPoint to)
        {
            try
            {
                return _socket.SendTo(message, length, SocketFlags.None, to);
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode == SocketError.WouldBlock)
                {
                    return 0;
                }

                throw;
            }
        }

        public int Receive(byte[] message)
        {
            try
            {
                return _socket.Receive(message);
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode == SocketError.WouldBlock)
                {
                    return 0;
                }

                throw;
            }
        }

        public int Receive(byte[] message, int length)
        {
            try
            {
                return _socket.Receive(message, length, SocketFlags.None);
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode == SocketError.WouldBlock)
                {
                    return 0;
                }

                throw;
            }
        }

        public int Receive(byte[] message, int position, int length)
        {
            return _socket.Receive(message, position, length, SocketFlags.None);
        }

        public int ReceiveFrom(byte[] message, ref EndPoint from)
        {
            return _socket.ReceiveFrom(message, ref from);
        }

        public int ReceiveFrom(byte[] message, ref IPEndPoint from)
        {
            EndPoint fromEp = @from;
            int ret = _socket.ReceiveFrom(message, ref fromEp);
            return ret;
        }

        public int Peek(byte[] message)
        {
            try
            {
                return _socket.Receive(message, SocketFlags.Peek);
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode == SocketError.WouldBlock)
                {
                    return 0;
                }

                throw;
            }
        }

        public bool Close()
        {
            try
            {
                OldHandle = (int) _socket.Handle;
                _socket.Close();
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode == SocketError.WouldBlock)
                {
                    return false;
                }

                throw;
            }

            return true;
        }

        public void Listen(int backlog)
        {
            _socket.Listen(backlog);
        }

        public EndPoint LocalEndPoint
        {
            get { return _socket.LocalEndPoint; }
        }

        public EndPoint RemoteEndPoint
        {
            get { return _socket.RemoteEndPoint; }
        }

        public bool IsBound
        {
            get { return _socket.IsBound; }
        }

        public ISocket Accept()
        {
            return new WrappedSocket(_socket.Accept());
        }

        public void SetSocketOption(SocketOptionLevel level, SocketOptionName name, Object value)
        {
            _socket.SetSocketOption(level, name, value);
        }

        #endregion

        //add this code to class ThreeDPoint as defined previously
        //
        public static bool operator ==(WrappedSocket a, Socket b)
        {
            if (a == null)
            {
                if (b == null)
                    return true;
                else
                    return false;
            }
            else if (b == null)
            {
                return false;
            }

            return a.Handle == b.Handle;
        }

        public static bool operator !=(WrappedSocket a, Socket b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return (_socket != null ? _socket.GetHashCode() : 0);
        }
        protected bool Equals(WrappedSocket other)
        {
            return Equals(_socket, other._socket);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((WrappedSocket)obj);
        }
    }
}