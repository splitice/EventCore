using System;
using System.Collections.Generic;
using System.Threading;

namespace EventCore.Threading
{
    /// <summary>
    /// A buffer like structure for communicating accross threads using a producer/consumer pattern
    /// </summary>
    /// <typeparam name="T">item type</typeparam>
    public class Channel<T>
    {
        private readonly Queue<T> _data = new Queue<T>();
        private readonly Semaphore _semaphore = new Semaphore(0, int.MaxValue);
        private const int MillisecondsSpin = 20;

        /// <summary>
        /// Put an item into the channel
        /// </summary>
        /// <param name="item"></param>
        public virtual void Put(T item)
        {
            lock (this)
            {
                _data.Enqueue(item);
            }

            try
            {
                _semaphore.Release();
            }
            catch (SemaphoreFullException)
            {
                for (int i = 0; i < 10; i++)
                {
                    try
                    {
                        _semaphore.Release();
                    }
                    catch (SemaphoreFullException)
                    {
                        Thread.Sleep(MillisecondsSpin);
                        continue;
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Take an item from the channel
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public virtual T Take(int time = -1)
        {
            bool r = _semaphore.WaitOne(time);

            if (!r)
            {
                throw new TimeoutException();
            }

            try
            {
                lock (this)
                {
                    return _data.Dequeue();
                }
            }
            catch (ThreadInterruptedException)
            {
                _semaphore.Release();
                throw;
            }
        }
    }
}