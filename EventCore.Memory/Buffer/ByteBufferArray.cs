using System;
using System.Collections;
using System.Collections.Generic;

namespace EventCore.Memory.Buffer
{
    public class ByteBufferArray : IByteBuffer
    {
        public const int DefaultBufferLength = 1024;

        private byte[] _data;

        public ByteBufferArray(int size = DefaultBufferLength)
        {
            _data = new byte[DefaultBufferLength];
            Start = 0;
            End = 0;
        }

        public ByteBufferArray(byte[] data, int start, int end, int size = DefaultBufferLength)
        {
            _data = data;
            Start = start;
            End = end;
        }

        public byte[] Data
        {
            get { return _data; }
            internal set { _data = value; }
        }

        #region IByteBuffer Members

        public int End { get; set; }
        public int Start { get; set; }

        public IByteBuffer Take()
        {
            var tempBuffer2 = new ByteBufferArray();
            byte[] data = Data;
            int start = Start;
            int end = End;

            Data = tempBuffer2.Data;
            Start = tempBuffer2.Start;
            End = tempBuffer2.End;

            tempBuffer2.Data = data;
            tempBuffer2.Start = start;
            tempBuffer2.End = end;

            return tempBuffer2;
        }

        public bool Done
        {
            get { return Start >= End; }
        }

        public int Length
        {
            get { return _data.Length; }
        }

        public int Count
        {
            get { return End - Start; }
        }


        public byte[] ToByte()
        {
            return _data;
        }

        public void Clear()
        {
            Start = 0;
            End = 0;
        }

        public int IndexOf(byte item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, byte item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public byte this[int index]
        {
            get { return _data[index + Start]; }
            set { _data[index + Start] = value; }
        }

        public void Add(byte item)
        {
            throw new NotImplementedException();
        }

        public bool Contains(byte item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(byte[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(byte item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<byte> GetEnumerator()
        {
            return (IEnumerator<byte>) _data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        public ArraySegment<byte> ToSegment(bool useRemaining)
        {
            return new ArraySegment<byte>(_data, Start, useRemaining ? (End - Start) : Count);
        }

        public void Write(string str)
        {
            //TODO: memcpy?
            foreach (char c in str)
            {
                _data[End] = (byte) c;
                End++;
            }
        }

        #endregion

        public static ByteBufferArray FromBufferData(byte[] data, int start, int end)
        {
            return new ByteBufferArray(data, start, end);
        }
    }
}