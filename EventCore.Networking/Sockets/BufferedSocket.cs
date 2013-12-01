using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using EventCore.Memory;
using EventCore.Memory.Buffer;

namespace EventCore.Networking.Sockets
{
    public class BufferedSocket : ISocket
    {
        private readonly bool _canBuffer;
        private readonly ISocket _socket;
        private IByteBuffer _byteBuffer;

        public BufferedSocket(AddressFamily family, SocketType socketType, ProtocolType protocolType)
        {
            _byteBuffer = null;
            _socket = new WrappedSocket(family, socketType, protocolType);
            _canBuffer = IsBufferable(socketType);
        }

        public BufferedSocket(Socket socket)
        {
            _byteBuffer = null;
            _socket = new WrappedSocket(socket);
            _canBuffer = IsBufferable(socket.SocketType);
        }

        internal BufferedSocket(ISocket socket)
        {
            _byteBuffer = null;
            _socket = socket;
            _canBuffer = IsBufferable(socket.Socket.SocketType);
        }

        public bool CanBuffer
        {
            get { return _canBuffer; }
        }

        public int BufferLength
        {
            get { return (_byteBuffer == null) ? 0 : _byteBuffer.Count; }
        }

        public bool HasBuffer
        {
            get { return BufferLength != 0; }
        }

        public Socket Socket
        {
            get { return _socket.Socket; }
        }

        public String ProtocolTypeString
        {
            get { return (ProtocolType == ProtocolType.Tcp) ? "tcp" : "udp"; }
        }

        public IPEndPoint LocalEndPointIp
        {
            get { return (IPEndPoint) _socket.LocalEndPoint; }
        }

        public IPEndPoint RemoteEndPointIP
        {
            get { return (IPEndPoint) RemoteEndPoint; }
        }

        #region ISocket Members

        public int OldHandle
        {
            get { return _socket.OldHandle; }
        }

        public int Peek(byte[] message, int offset, int count)
        {
            return _socket.Peek(message, offset, count);
        }

        public int ReceiveFrom(byte[] message, int offset, int count, ref EndPoint to)
        {
            return _socket.ReceiveFrom(message, offset, count, ref to);
        }

        public int SendTo(byte[] message, int offset, int count, IPEndPoint to)
        {
            return _socket.SendTo(message, offset, count, to);
        }

        public int SendTo(byte[] message, int offset, int count, EndPoint to)
        {
            return _socket.SendTo(message, offset, count, to);
        }

        public int Send(byte[] message, int start, int length)
        {
            if (_byteBuffer != null)
            {
                throw new Exception("Buffer must be cleared before calling a non buffered send!");
            }
            return _socket.Send(message, start, length);
        }

        public IntPtr Handle
        {
            get { return _socket.Handle; }
        }

        Socket ISocket.Socket
        {
            get { return _socket.Socket; }
        }

        public ProtocolType ProtocolType
        {
            get { return _socket.ProtocolType; }
        }

        public bool Connected
        {
            get { return _socket.Connected; }
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

        public void Bind(IPEndPoint from)
        {
            _socket.Bind(from);
        }

        public void Bind(IPAddress from)
        {
            _socket.Bind(from);
        }

        public bool Poll(SelectMode mode)
        {
            return _socket.Poll(mode);
        }

        public bool Connect(IPEndPoint to)
        {
            return _socket.Connect(to);
        }

        public int Send(IList<ArraySegment<byte>> message)
        {
            return _socket.Send(message);
        }

        public int Send(IList<ArraySegment<byte>> message, int length)
        {
            return _socket.Send(message, length);
        }

        public int Send(IList<ArraySegment<byte>> message, int start, int length)
        {
            return _socket.Send(message, start, length);
        }

        public int Send(byte[] message, int length)
        {
            return _socket.Send(message, length);
        }

        public int SendTo(byte[] message, int length, IPEndPoint to)
        {
            return _socket.SendTo(message, length, to);
        }

        public int SendTo(byte[] message, int length, EndPoint to)
        {
            return _socket.SendTo(message, length, to);
        }

        public int Receive(byte[] message)
        {
            return _socket.Receive(message);
        }

        public int Receive(byte[] message, int length)
        {
            return _socket.Receive(message, length);
        }

        public int Receive(byte[] message, int position, int length)
        {
            return _socket.Receive(message, position, length);
        }

        public int ReceiveFrom(byte[] message, ref IPEndPoint from)
        {
            return _socket.ReceiveFrom(message, ref from);
        }

        public int ReceiveFrom(byte[] message, ref EndPoint from)
        {
            return _socket.ReceiveFrom(message, ref from);
        }

        public int Peek(byte[] message)
        {
            return _socket.Peek(message);
        }

        public bool Close()
        {
            return _socket.Close();
        }

        public void Listen(int backlog)
        {
            _socket.Listen(backlog);
        }

        ISocket ISocket.Accept()
        {
            return _socket.Accept();
        }

        public void Shutdown(SocketShutdown shutdown)
        {
            _socket.Shutdown(shutdown);
        }

        public void SetSocketOption(SocketOptionLevel level, SocketOptionName name, object value)
        {
            _socket.SetSocketOption(level, name, value);
        }

        #endregion

        private static bool IsBufferable(SocketType type)
        {
            return (type == SocketType.Stream);
        }

        /*~BufferedSocket()
        {
            if (_byteBuffer != null)
                BufferManager.Instance.Return(_byteBuffer);
        }*/

        public BufferedSocket Accept()
        {
            return new BufferedSocket(Socket.Accept());
        }

        public IByteBuffer TakeBuffer()
        {
            IByteBuffer byteBuffer = _byteBuffer;
            _byteBuffer = null;
            return byteBuffer;
        }

        public int Read(IByteBuffer buffer)
        {
            buffer.Clear();
            ArraySegment<byte> segment = buffer.ToSegment(true);
            buffer.End = _socket.Receive(segment.Array, segment.Offset, segment.Count) + buffer.Start;
            return buffer.End;
        }

        public int PeekRead(IByteBuffer buffer)
        {
            buffer.Clear();
            ArraySegment<byte> segment = buffer.ToSegment(true);
            return buffer.End = _socket.Peek(segment.Array, segment.Offset, segment.Count) + buffer.Start;
        }

        public int SendBuffered(ref byte[] message, int length)
        {
            return SendBuffered(ref message, 0, length);
        }

        public int SendBuffered(ref byte[] message, int start, int length)
        {
            if (!_canBuffer)
            {
                return Send(message, start, length);
            }
            if (HasBuffer)
            {
                throw new Exception("Buffer must be cleared before calling a non buffered send!");
            }

            int count = _socket.Send(message, start, length);
            if (count != length)
            {
                //Creates a new buffer, will never be returned
                _byteBuffer = ByteBufferArray.FromBufferData(message, count + start, start + length - count);
            }
            return count;
        }

        public int SendBuffered(IList<ArraySegment<byte>> message)
        {
            if (!_canBuffer)
            {
                return Send(message);
            }
            if (_byteBuffer != null)
            {
                throw new Exception("Buffer must be cleared before calling a non buffered send!");
            }
            int count = _socket.Send(message);
            int ret = count;

            bool copyToBufferState = false;
            List<byte> bufferBytes = null;
            foreach (var c in message)
            {
                if (copyToBufferState)
                {
                    bufferBytes.AddRange(c.Array.Skip(c.Offset).Take(c.Count));
                }
                else
                {
                    if (c.Count <= count)
                    {
                        count--;
                    }
                    else
                    {
                        bufferBytes = c.Array.Skip(c.Offset + c.Count - count).ToList();
                        copyToBufferState = true;
                    }
                }
            }

            //Move to buffer
            if (bufferBytes != null)
            {
                _byteBuffer = new ByteBufferArray(bufferBytes.ToArray(), 0, bufferBytes.Count);
            }


            return ret;
        }

        public int SendBuffered(IByteBuffer message)
        {
            ArraySegment<byte> segment = message.ToSegment(false);
            if (!_canBuffer)
            {
                return Send(segment.Array, segment.Offset, segment.Count);
            }
            if (HasBuffer)
            {
                throw new Exception("Buffer must be cleared before calling a non buffered send!");
            }
            int count = _socket.Send(segment.Array, segment.Offset, segment.Count);
            message.Start += count;

            if (message.Count != 0)
            {
                _byteBuffer = message.Take();
                ;
            }

            return count;
        }

        public int ReceiveFrom(IByteBuffer message, ref EndPoint to)
        {
            message.Clear();
            ArraySegment<byte> segment = message.ToSegment(true);
            int length = _socket.ReceiveFrom(segment.Array, segment.Offset, segment.Count, ref to);
            message.End = length;
            return length;
        }

        public int ReceiveFrom(IByteBuffer message, ref IPEndPoint to)
        {
            message.Clear();
            EndPoint ep = to;
            int length = ReceiveFrom(message, ref ep);
            to = (IPEndPoint) ep;
            return length;
        }

        public int SendTo(IByteBuffer message, IPEndPoint to)
        {
            ArraySegment<byte> segment = message.ToSegment(false);
            return _socket.SendTo(segment.Array, segment.Offset, segment.Count, to);
        }

        public int SendTo(IByteBuffer message, EndPoint to)
        {
            ArraySegment<byte> segment = message.ToSegment(false);
            return _socket.SendTo(segment.Array, segment.Offset, segment.Count, to);
        }

        public int SendToBuffered(byte[] message, int length, IPEndPoint to)
        {
            throw new NotImplementedException();
            return SendToBuffered(message, length, (EndPoint) to);
        }

        public int SendToBuffered(byte[] message, int length, EndPoint to)
        {
            throw new NotImplementedException();
            int count = SendTo(message, length, to);
            return count;
        }


        public bool AttemptFlushBuffer(out int count)
        {
            ArraySegment<byte> segment = _byteBuffer.ToSegment(false);
            count = _socket.Send(segment.Array, segment.Offset, segment.Count);
            _byteBuffer.Start += count;
            if (_byteBuffer.Done)
            {
                BufferManager.Instance.Return(_byteBuffer);
                _byteBuffer = null;
            }
            return HasBuffer;
        }


        public bool AttemptFlushBuffer()
        {
            int count;
            return AttemptFlushBuffer(out count);
        }
    }
}