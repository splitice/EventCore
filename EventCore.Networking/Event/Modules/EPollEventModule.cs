using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using EventCore.Networking.Event.Modules.EPoll;
using EventCore.Networking.Sockets;
using EventCore.Networking.Timer;
using Mono.Unix.Native;

namespace EventCore.Networking.Event.Modules
{
    public class EPollEventModule : EventHelpers, IEvent
    {
        internal const EpollEvents ReadEvent = EpollEvents.EPOLLIN;
        internal const EpollEvents WriteEvent = EpollEvents.EPOLLOUT;

        private readonly EPollQueue _epoll;

        private readonly Dictionary<int, Action> _onAcceptSockets =
            new Dictionary<int, Action>();

        private readonly Dictionary<int, Action> _onReadSockets = new Dictionary<int, Action>();
        private readonly Dictionary<int, Action> _onWriteSockets = new Dictionary<int, Action>();

        public EPollEventModule(int maxEvents = 256)
        {
            Contract.Requires(maxEvents > 0);

            _epoll = new EPollQueue(maxEvents);
            Timer = new EventTimer();
        }

        #region IEvent Members

        public void RegisterSocketRead(BufferedSocket socket, Action callback)
        {
#if DEBUG_VERBOSE
            Console.WriteLine("RegisterSocketRead");
#endif
            var handle = (int) socket.Handle;
            if (handle == -1)
            {
                Console.WriteLine("RegisterSocketRead socket already closed");
                return;
            }

            if (!AddSocket(handle, EpollEvents.EPOLLIN))
            {
            }

#if DEBUG
            if (_onReadSockets.ContainsKey(handle))
                Console.WriteLine("Already waiting on read from socket");
#endif
            if (_onReadSockets.ContainsKey(handle))
                _onReadSockets[handle] = callback;
            else
                _onReadSockets.Add(handle, callback);
        }

        public void RegisterSocketWrite(BufferedSocket socket, Action callback)
        {
#if DEBUG_VERBOSE
            Console.WriteLine("RegisterSocketWrite");
#endif
            var handle = (int) socket.Handle;
            if (handle == -1)
            {
                Console.WriteLine("RegisterSocketWrite socket already closed");
                return;
            }

            if (!AddSocket(handle, EpollEvents.EPOLLOUT))
            {
            }
#if DEBUG
            if (_onWriteSockets.ContainsKey((int) socket.Handle))
                Console.WriteLine("Already waiting on write from socket");
#endif
            if (_onWriteSockets.ContainsKey(handle))
                _onWriteSockets[handle] = callback;
            else
                _onWriteSockets.Add(handle, callback);
        }

        public void RegisterSocketAccept(BufferedSocket socket, Action<BufferedSocket> callback)
        {
#if DEBUG_VERBOSE
            Console.WriteLine("RegisterSocketAccept");
#endif

            var handle = (int) socket.Handle;
            if (handle == -1)
            {
                Console.WriteLine("RegisterSocketWrite socket already closed");
                return;
            }
            if (!AddSocket(handle, EpollEvents.EPOLLIN))
            {
            }
            Action acceptHandle = () =>
                                      {
                                          BufferedSocket s2 = null;
                                          try
                                          {
                                              s2 = socket.Accept();
                                          }
                                          catch (Exception)
                                          {
                                              //TODO: unregister uing intger handle
                                              UnregisterSocketAccept(socket);
                                              return;
                                          }

                                          callback(s2);
                                      };

            if (_onAcceptSockets.ContainsKey(handle))
                _onAcceptSockets[handle] = acceptHandle;
            else
                _onAcceptSockets.Add(handle, acceptHandle);
        }

        public void UpdateSocketAccept(BufferedSocket socket, Action<BufferedSocket> callback)
        {
            var handle = (int) socket.Handle;
            if (handle == -1)
            {
                Console.WriteLine("UpdateSocketAccept socket already closed");
                return;
            }
            Action acceptHandle = () =>
                                      {
                                          BufferedSocket s2 = null;
                                          try
                                          {
                                              s2 = socket.Accept();
                                          }
                                          catch (Exception)
                                          {
                                              UnregisterSocketAccept(socket);
                                              return;
                                          }

                                          callback(s2);
                                      };

            if (_onAcceptSockets.ContainsKey(handle))
                _onAcceptSockets[handle] = acceptHandle;
        }

        public void UnregisterSocketRead(BufferedSocket socket)
        {
#if DEBUG_VERBOSE
            Console.WriteLine("UnregisterSocketRead");
#endif
            var handle = (int) socket.Handle;
            if (handle == -1)
            {
                //if (_onReadSockets.ContainsKey(socket.OldHandle))
                //    _onReadSockets.Remove(socket.OldHandle);
                Console.WriteLine("UnregisterSocketRead socket already closed");
                return;
            }
            _onReadSockets.Remove(handle);

            if (!RemoveSocket(handle, EpollEvents.EPOLLIN))
            {
            }
        }

        public void UnregisterSocketWrite(BufferedSocket socket)
        {
#if DEBUG_VERBOSE
            Console.WriteLine("UnregisterSocketWrite");
#endif
            var handle = (int) socket.Handle;

            if (handle == -1)
            {
                //if (_onReadSockets.ContainsKey(socket.OldHandle))
                //    _onReadSockets.Remove(socket.OldHandle);
                Console.WriteLine("UnregisterSocketWrite socket already closed");
                return;
            }

            _onWriteSockets.Remove(handle);

            if (!RemoveSocket(handle, EpollEvents.EPOLLOUT))
            {
            }
        }

        public void UnregisterSocketAccept(BufferedSocket socket)
        {
#if DEBUG_VERBOSE
            Console.WriteLine("UnregisterSocketAccept");
#endif
            var handle = (int) socket.Handle;

            if (handle == -1)
            {
                //if (_onAcceptSockets.ContainsKey(socket.OldHandle))
                //    _onAcceptSockets.Remove(socket.OldHandle);
                Console.WriteLine("UnregisterSocketAccept socket already closed");
                return;
            }

            _onAcceptSockets.Remove(handle);
            _epoll.Delete(handle);
        }

        public void UnregisterSocket(BufferedSocket socket)
        {
#if DEBUG_VERBOSE
            Console.WriteLine("UnregisterSocket");
#endif
            var fd = (int) socket.Handle;
            if (_onReadSockets.ContainsKey(fd))
                UnregisterSocketRead(socket);
            if (_onWriteSockets.ContainsKey(fd))
                UnregisterSocketWrite(socket);
            if (_onAcceptSockets.ContainsKey(fd))
                UnregisterSocketAccept(socket);
        }

        public EventTimer Timer { get; private set; }
        public DateTime StartOfRun { get; private set; }

        public void Run()
        {
            int ms = Timer.WaitForMilliseconds(DateTime.Now,50);

            int events = _epoll.Execute(ms);
            StartOfRun = DateTime.Now;

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

            foreach (EpollEvent e in _epoll.Events.Take(events))
            {
                try
                {
                    if (_onAcceptSockets.ContainsKey(e.fd))
                    {
                        Contract.Assert(_onAcceptSockets[e.fd] != null);
                        _onAcceptSockets[e.fd]();
                    }
                    else
                    {
                        if ((e.events & EpollEvents.EPOLLIN) != 0 && _onReadSockets.ContainsKey(e.fd))
                        {
                            Contract.Assert(_onReadSockets[e.fd] != null);
                            _onReadSockets[e.fd]();
                        }
                        else if ((e.events & EpollEvents.EPOLLOUT) != 0 && _onWriteSockets.ContainsKey(e.fd))
                        {
                            Contract.Assert(_onWriteSockets[e.fd] != null);
                            _onWriteSockets[e.fd]();
                        }
                        else
                        {
                            //Who knows why?
                            _epoll.Delete(e.fd);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unhandled Exception processing event: " + ex.Message);
                    Console.WriteLine(ex.StackTrace);
#if DEBUG
                    throw;
#endif
                }
            }
        }

        #endregion

        private bool AddSocket(int handle, EpollEvents e)
        {
            bool
                isWrite = _onWriteSockets.ContainsKey(handle),
                isRead = _onReadSockets.ContainsKey(handle);

            Contract.Assert(!((e & ReadEvent) == ReadEvent && isRead),
                            "adding to read list, although already added");
            Contract.Assert(!((e & WriteEvent) == WriteEvent && isWrite),
                            "adding to write list, although already added");

            if (isWrite && (e & ReadEvent) == ReadEvent ||
                isRead && (e & WriteEvent) == WriteEvent)
            {
                _epoll.Edit(handle, ReadEvent | WriteEvent);
            }
            else
            {
                _epoll.Add(handle, e);
            }

            return true;
        }

        private bool RemoveSocket(int handle, EpollEvents e)
        {
            int res;
            bool
                isWrite = _onWriteSockets.ContainsKey(handle),
                isRead = _onReadSockets.ContainsKey(handle);

            Contract.Assert(!((e & ReadEvent) == ReadEvent && !isRead),
                            "removing read from list, although already not added");
            Contract.Assert(!((e & WriteEvent) == WriteEvent && !isWrite),
                            "removing write from list, although already not added");

            if (isWrite && (e & ReadEvent) == ReadEvent ||
                isRead && (e & WriteEvent) == WriteEvent)
            {
                EpollEvents inv = ((e & ReadEvent) == ReadEvent)
                                      ? WriteEvent
                                      : ReadEvent;

                res = _epoll.Edit(handle, inv);
            }
            else
            {
                res = _epoll.Delete(handle);
            }

            if (res == (int) Errno.EINTR)
            {
                return RemoveSocket(handle, e);
            }

            return true;
        }

        ~EPollEventModule()
        {
            _epoll.Close();
        }
    }
}