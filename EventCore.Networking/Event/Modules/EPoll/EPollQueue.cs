using System;
using System.Collections.Generic;
using Mono.Unix.Native;

namespace EventCore.Networking.Event.Modules.EPoll
{
    internal class EPollQueue
    {
        private readonly EpollEvent[] _events;
        private int _epoll;

        internal EPollQueue(int maxEvents = 64)
        {
            _events = new EpollEvent[maxEvents];
            _epoll = Syscall.epoll_create(maxEvents);
        }

        public IEnumerable<EpollEvent> Events
        {
            get { return _events; }
        }

        internal int Edit(int handle, EpollEvents events)
        {
            if (_epoll == 0)
            {
                throw new InvalidOperationException("Epoll has already been closed");
            }

            int res = Syscall.epoll_ctl(_epoll, EpollOp.EPOLL_CTL_MOD, handle, events);
            return res;
        }

        internal int Add(int handle, EpollEvents events)
        {
            if (_epoll == 0)
            {
                throw new InvalidOperationException("Epoll has already been closed");
            }

            int res = Syscall.epoll_ctl(_epoll, EpollOp.EPOLL_CTL_ADD, handle, events);
            return res;
        }

        internal int Delete(int handle)
        {
            if (_epoll == 0)
            {
                throw new InvalidOperationException("Epoll has already been closed");
            }

            int res = Syscall.epoll_ctl(_epoll, EpollOp.EPOLL_CTL_DEL, handle, 0);
            return res;
        }

        public int Execute(int ms = 500)
        {
            int events = Syscall.epoll_wait(_epoll, _events, _events.Length, ms);
            if (events == -1)
            {
                Errno error = Stdlib.GetLastError();
                if (error != Errno.EINTR)
                {
                    throw new InvalidOperationException(String.Format("Error {0} occured while waiting for an event",
                                                                      error));
                }
            }

            return events;
        }

        public void Close()
        {
            if (_epoll != 0)
            {
                Syscall.close(_epoll);
                _epoll = 0;
            }
        }

        ~EPollQueue()
        {
            Close();
        }
    }
}