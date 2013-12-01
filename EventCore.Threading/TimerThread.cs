using System;
using System.Collections.Generic;
using System.Threading;
using X4B.Common.Types;

namespace X4B.Common.Threading
{
    public class TimerThread
    {
        private readonly MultiValueDictionary<DateTime, Action> _callbacks =
            new MultiValueDictionary<DateTime, Action>();

        private readonly Timer _timer;
        private readonly PriorityQueue<long, DateTime> _times;
        private DateTime? _current;

        public TimerThread()
        {
            _timer = new Timer(Callback);
            _times = new PriorityQueue<long, DateTime>();
            _current = null;
        }

        public void RemvoveCallback(DateTime time)
        {
            lock (_callbacks)
            {
                _callbacks.Remove(time);
            }

            long next = NextTime();
            lock (_timer)
            {
                //Incase the item being removed is th next time
                _timer.Change(next, Timeout.Infinite);
            }
        }

        private long NextTime()
        {
            if (_times.Count == 0)
            {
                _current = null;
                return Timeout.Infinite;
            }

            DateTime time;
            lock (_times)
            {
                time = _times.DequeueValue();
            }
            _current = time;

            var ms = (long) time.Subtract(DateTime.Now).TotalMilliseconds;
            if (ms < 0)
                return 0;
            return ms;
        }


        public TimeEvent AddTime(int milliseconds, Action callback)
        {
            //The time is in the future by milliseconds ms
            DateTime time = DateTime.Now.AddMilliseconds(milliseconds);
            lock (_callbacks)
            {
                _callbacks.Add(time, callback);
            }
            lock (_times)
            {
                _times.Enqueue(time.Ticks, time);
            }
            if (_current == null)
            {
                lock (_timer)
                {
                    _timer.Change(NextTime(), Timeout.Infinite);
                }
            }
            else if (_current.Value < time)
            {
                lock (_times)
                {
                    _times.Enqueue(_current.Value.Ticks, _current.Value);
                }

                long next = NextTime();
                lock (_timer)
                {
                    _timer.Change(next, Timeout.Infinite);
                }
            }

            return new TimeEvent(time);
        }

        /// <summary>
        /// Called by the timer everytime it thinks there is an event due.
        /// </summary>
        /// <param name="o">The o.</param>
        protected void Callback(object o)
        {
            //Work out what we need, while locked
            var toRun = new List<Action>();
            var toRemove = new List<DateTime>();
            lock (_callbacks)
            {
                DateTime now = DateTime.Now;
                foreach (var pair in _callbacks)
                {
                    if (pair.Key < now)
                    {
                        toRun.AddRange(pair.Value);
                        toRemove.Add(pair.Key);
                    }
                }
            }

            //Execute the removal
            foreach (DateTime date in toRemove)
            {
                _callbacks.Remove(date);
            }

            //Execute the run
            foreach (Action a in toRun)
            {
                a();
            }

            //Schedule
            long next = NextTime();
            lock (_timer)
            {
                _timer.Change(next, Timeout.Infinite);
            }
        }

        public TimeEvent AddInterval(int i, Action run)
        {
            return AddTime(i, run);
        }

        #region Nested type: IntevalTimeEvent

        public class IntevalTimeEvent
        {
        }

        #endregion

        #region Nested type: TimeEvent

        /// <summary>
        /// An structure containing the details of a timer event
        /// </summary>
        public class TimeEvent
        {
            private readonly DateTime _time;

            public TimeEvent(DateTime time)
            {
                _time = time;
            }

            public void Remove(TimerThread tt)
            {
                tt.RemvoveCallback(_time);
            }
        }

        #endregion
    }
}