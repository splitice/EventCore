using System;

namespace EventCore.Networking.Timer
{
    public class EventTimerEvent
    {
        private Action _callback;
        public int Interval;

        public EventTimerEvent(Action callback, int interval)
        {
            _callback = callback;
            Interval = interval;
        }

        /// <summary>
        /// Null out this event, it will be cleaned up later
        /// </summary>
        public void LazyRemove()
        {
            _callback = null;
            Interval = 0;
        }

        public void Execute()
        {
            if (_callback != null)
                _callback();
        }
    }
}