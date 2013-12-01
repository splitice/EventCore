using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using EventCore.Networking.Sockets;
using EventCore.Networking.Timer;

namespace EventCore.Networking.Event.Modules
{
    public class SelectEventModule : EventHelpers, IEvent
    {
        private readonly Dictionary<BufferedSocket, Thread> _onAcceptSockets = new Dictionary<BufferedSocket, Thread>();

        private readonly Dictionary<BufferedSocket, Action> _onReadSockets =
            new Dictionary<BufferedSocket, Action>();

        private readonly Dictionary<BufferedSocket, Action> _onWriteSockets =
            new Dictionary<BufferedSocket, Action>();

        public SelectEventModule()
        {
            Timer = new EventTimer();
        }

        public List<BufferedSocket> Sockets
        {
            get
            {
                var sockets = new List<BufferedSocket>();
                sockets.AddRange(_onReadSockets.Keys);
                sockets.AddRange(_onWriteSockets.Keys);
                return new List<BufferedSocket>(new HashSet<BufferedSocket>(sockets)); //Bad bad
            }
        }

        #region IEvent Members

        public void RegisterSocketRead(BufferedSocket socket, Action callback)
        {
            lock (this)
            {
#if DEBUG
                if (_onReadSockets.ContainsKey(socket))
                {
                    throw new Exception("Socket already added!");
                }
#endif
                _onReadSockets.Add(socket, callback);
            }
        }

        public void RegisterSocketWrite(BufferedSocket socket, Action callback)
        {
            lock (this)
            {
                _onWriteSockets.Add(socket, callback);
            }
        }

        public void RegisterSocketAccept(BufferedSocket socket, Action<BufferedSocket> callback)
        {
            var thread = new Thread(() =>
                                        {
                                            try
                                            {
                                                socket.Socket.Blocking = true;
                                                while (socket.IsBound)
                                                {
                                                    BufferedSocket newConnection = socket.Accept();
                                                    //newConnection.Blocking = false;
                                                    lock (this)
                                                    {
                                                        Thread.BeginCriticalRegion();
                                                        callback(newConnection);
                                                        Thread.EndCriticalRegion();
                                                    }
                                                }
                                            }
                                            catch (ThreadAbortException)
                                            {
                                                Console.WriteLine("Accept for socket Aborted");
                                                return;
                                            }
                                            catch (SocketException ex)
                                            {
                                                if (ex.ErrorCode == 10004)
                                                {
                                                    Console.WriteLine("Accept for socket Aborted");
                                                    return;
                                                }
                                                else
                                                {
                                                    Console.WriteLine("Unknown exception in accept thread: " +
                                                                      ex.Message);
                                                }
                                            }
                                        });
            thread.Name = "Socket Accept " + socket.LocalEndPointIp;
            _onAcceptSockets.Add(socket, thread);
            thread.Start();
        }

        public void UpdateSocketAccept(BufferedSocket socket, Action<BufferedSocket> callback)
        {
            UnregisterSocketAccept(socket);
            RegisterSocketAccept(socket, callback);
        }

        public void UnregisterSocketRead(BufferedSocket socket)
        {
            lock (this)
            {
                _onReadSockets.Remove(socket);
            }
        }

        public void UnregisterSocketWrite(BufferedSocket socket)
        {
            lock (this)
            {
                _onWriteSockets.Remove(socket);
            }
        }

        public void UnregisterSocketAccept(BufferedSocket socket)
        {
            if (!_onAcceptSockets.ContainsKey(socket))
            {
                throw new ArgumentOutOfRangeException("socket", "BufferedSocket does not exist in accept list");
            }
            _onAcceptSockets[socket].Abort();
            _onAcceptSockets.Remove(socket);
        }

        public void UnregisterSocket(BufferedSocket socket)
        {
            lock (this)
            {
                if (_onReadSockets.ContainsKey(socket))
                    _onReadSockets.Remove(socket);
                if (_onWriteSockets.ContainsKey(socket))
                    _onWriteSockets.Remove(socket);
                if (_onAcceptSockets.ContainsKey(socket))
                    _onAcceptSockets[socket].Abort();
            }
        }

        public void Run()
        {
            if (_onWriteSockets.Count == 0 && _onReadSockets.Count == 0)
            {
                Thread.Sleep(500);
                return;
            }
            //Console.WriteLine(String.Format("There are currently {0} read sockets, {1} write sockets", _onReadSockets.Count, _onWriteSockets.Count));

            //Do a select
            Dictionary<Socket, BufferedSocket> dictioary;
            List<Socket> toRead, toWrite;
            lock (this)
            {
                dictioary = _onReadSockets.Keys.ToDictionary(o => o.Socket, o => o);
                toRead = new List<Socket>(_onReadSockets.Keys.Select(a => a.Socket));

                foreach (BufferedSocket s in _onWriteSockets.Keys)
                {
                    if (!dictioary.ContainsKey(s.Socket))
                        dictioary.Add(s.Socket, s);
                }
                toWrite = new List<Socket>(_onWriteSockets.Keys.Select(a => a.Socket));
            }

            int ms = Timer.WaitForMilliseconds(StartOfRun);
            try
            {
                Socket.Select(toRead, toWrite, null, ms);
            }
            catch (ObjectDisposedException)
            {
                Console.WriteLine("A socket in the list had been cleaned up while we where waiting.");
                Run();
            }

            try
            {
                Timer.Run(StartOfRun);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unhandled Exception processing timers: " + ex.Message);
#if DEBUG
                throw;
#endif
            }

            try
            {
                foreach (Socket socket in toRead)
                {
                    BufferedSocket isocket = dictioary[socket];
                    if (_onReadSockets.ContainsKey(isocket))
                    {
                        lock (this)
                        {
                            _onReadSockets[isocket]();
                        }
                    }
                }

                foreach (Socket socket in toWrite)
                {
                    BufferedSocket isocket = dictioary[socket];
                    if (_onWriteSockets.ContainsKey(isocket))
                    {
                        lock (this)
                        {
                            _onWriteSockets[isocket]();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unhandled Exception processing event: " + ex.Message);
#if DEBUG
                throw;
#endif
            }
        }

        public EventTimer Timer { get; private set; }

        public DateTime StartOfRun
        {
            get { return DateTime.Now; }
        }

        #endregion
    }
}