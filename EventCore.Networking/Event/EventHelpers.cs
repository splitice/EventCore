using System;
using System.Net;
using System.Net.Sockets;
using EventCore.Common;
using EventCore.Common.Types;
using EventCore.Memory;
using EventCore.Memory.Buffer;
using EventCore.Networking.Sockets;

namespace EventCore.Networking.Event
{
    public class EventHelpers : Singleton<EventHelpers>, IEventHelpers
    {
        private readonly IByteBuffer _msg = BufferManager.Instance.Take();

        #region IEventHelpers Members

        public void RegisterSocketRead(IEvent e, BufferedSocket socket, IEventHandler protocol, ReadFunction callback)
        {
            e.RegisterSocketRead(socket, () =>
                                             {
                                                 EndPoint ep = new IPEndPoint(IPAddress.Any, 0);
                                                 try
                                                 {
                                                     protocol.Read(socket, _msg, ref ep);
                                                 }
                                                 catch (SocketException ex)
                                                 {
                                                     Console.WriteLine("SocketException occured: " + ex.Message);
                                                     _msg.Clear();
                                                 }
                                                 callback(_msg, ep);
                                             });
        }

        public void RegisterSocketPeekRead(IEvent e, BufferedSocket socket, IEventHandler protocol,
                                           ReadFunction callback)
        {
            e.RegisterSocketRead(socket, () =>
                                             {
                                                 EndPoint ep = new IPEndPoint(IPAddress.Any, 0);
                                                 try
                                                 {
                                                     protocol.Peek(socket, _msg, ref ep);
                                                 }
                                                 catch (SocketException)
                                                 {
                                                     _msg.Clear();
                                                 }
                                                 callback(_msg, ep);
                                             });
        }

        #endregion
    }
}