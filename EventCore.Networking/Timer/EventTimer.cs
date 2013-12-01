using System;
using System.Collections.Generic;
using System.Linq;
using EventCore.Common;
using EventCore.Common.Types;

namespace EventCore.Networking.Timer
{
    public class EventTimer
    {
        private readonly PriorityQueue<DateTime, EventTimerEvent> _queue =
            new PriorityQueue<DateTime, EventTimerEvent>();

        public EventTimerEvent AddTime(int milliseconds, Action then, int interval = 0)
        {
            DateTime time = DateTime.Now.AddMilliseconds(milliseconds);
            var e = new EventTimerEvent(then, interval);
            _queue.Add(new KeyValuePair<DateTime, EventTimerEvent>(time, e));
            return e;
        }

        public EventTimerEvent AddInterval(int milliseconds, Action then)
        {
            return AddTime(milliseconds, then, milliseconds);
        }

        public int WaitForMilliseconds(DateTime now, int msMax = 100)
        {
            if (_queue.Count == 0)
            {
                return msMax;
            }
            KeyValuePair<DateTime, EventTimerEvent> first = _queue.FirstOrDefault();
            var msdiff = (int) (first.Key - now).TotalMilliseconds;
            if (msdiff > 0)
                return 1;

            if (msdiff > msMax)
                return msMax;

            return msdiff;
        }

        public void Run(DateTime now)
        {
            for (KeyValuePair<DateTime, EventTimerEvent> i = _queue.FirstOrDefault();
                 _queue.Count != 0 && i.Key <= now;
                 i = _queue.FirstOrDefault())
            {
                EventTimerEvent value = i.Value;
                value.Execute();
                _queue.Remove(i);
                if (value.Interval != 0)
                {
                    _queue.Add(new KeyValuePair<DateTime, EventTimerEvent>(i.Key.AddMilliseconds(value.Interval), value));
                }
            }
        }
    }
}