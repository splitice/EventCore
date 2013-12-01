using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

namespace EventCore.Memory.Buffer
{
    public class ByteBufferSlab : IByteBuffer
    {
        private readonly SlabDispenser _slab;
        private int _end;
        private int _position;
        private ArraySegment<byte> _segment;

        public ByteBufferSlab(ArraySegment<byte> segment, SlabDispenser slab)
        {
            _segment = segment;
            _slab = slab;
            _position = 0;
        }

        internal ArraySegment<byte> Segment
        {
            get { return _segment; }
            set { _segment = value; }
        }

        protected int Remaining
        {
            get { return _segment.Count - _position; }
        }

        #region IByteBuffer Members

        public IByteBuffer Take()
        {
            ByteBufferSlab slab = _slab.Take(0);
            if (slab == null)
            {
                //We need to copy :(
                var dest = new byte[Count];
                Array.Copy(_segment.Array, _segment.Offset + _position, dest, 0, dest.Length);
                return ByteBufferArray.FromBufferData(dest, 0, dest.Length);
            }

            int cStart = slab.Start;
            int cEnd = slab.End;
            ArraySegment<byte> cSegment = slab.Segment;

            slab.Start = Start;
            slab.End = End;
            slab.Segment = Segment;

            Start = cStart;
            End = cEnd; //Should always be 0
            Segment = cSegment;

            return slab;
        }

        public ArraySegment<byte> ToSegment(bool useRemaining)
        {
            return new ArraySegment<byte>(_segment.Array, _segment.Offset + _position, useRemaining ? Remaining : Count);
        }

        public int Length
        {
            get { return _slab.Length; }
        }

        public byte[] ToByte()
        {
            throw new NotImplementedException();
        }

        public IEnumerator<byte> GetEnumerator()
        {
            return new BytePointerEnumerator(_segment, _position, Count);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Non-implemented part of IList
        /// </summary>
        /// <param name="item"></param>
        public void Add(byte item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            _position = 0;
            _end = 0;
        }

        /// <summary>
        /// Non-implemented part of IList
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(byte item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Non-implemented part of IList
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(byte[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Non-implemented part of IList
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove(byte item)
        {
            throw new NotImplementedException();
        }


        public int Count
        {
            get { return _end - _position; }
        }

        public bool IsReadOnly
        {
            get { return false; }
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
            get { return _segment.Array[_position + _segment.Offset + index]; }
            set { _segment.Array[_position + _segment.Offset + index] = value; }
        }

        public bool Done
        {
            get { return Count == 0; }
        }

        public int Start
        {
            get { return _position; }
            set { _position = value; }
        }

        public int End
        {
            get { return _end; }
            set { _end = value; }
        }

        public void Write(string str)
        {
            if (str.Length > Remaining)
                throw new IndexOutOfRangeException("Attempt to write too many bytes to fixed sized Buffer Slab");

            foreach (char c in str)
            {
                _segment.Array[_end + _segment.Offset] = (byte) c;
                _end++;
            }
        }

        #endregion

        ~ByteBufferSlab()
        {
            _slab.Return(this);
        }
    }

    [TestFixture]
    internal class TestSlabBuffer
    {
        private readonly SlabDispenser dispenser = new SlabDispenser(2, 100);

        [TestCase]
        public void TestCopyToSlab()
        {
            ByteBufferSlab slab = dispenser.Take(0);
            slab.Write("Test");

            Assert.AreEqual((byte) 'T', slab[0]);
            Assert.AreEqual((byte) 'e', slab[1]);
            Assert.AreEqual((byte) 's', slab[2]);
            Assert.AreEqual((byte) 't', slab[3]);

            IByteBuffer newBuffer = slab.Take();

            Assert.AreEqual((byte) 'T', newBuffer[0]);
            Assert.AreEqual((byte) 'e', newBuffer[1]);
            Assert.AreEqual((byte) 's', newBuffer[2]);
            Assert.AreEqual((byte) 't', newBuffer[3]);

            newBuffer.Start = 1;

            Assert.AreEqual((byte) 'e', newBuffer[0]);
            Assert.AreEqual((byte) 's', newBuffer[1]);
            Assert.AreEqual((byte) 't', newBuffer[2]);

            dispenser.Return(slab);
        }

        [TestCase]
        public void TestCopyToByteArray()
        {
            ByteBufferSlab slabUnused = dispenser.Take(0);
            ByteBufferSlab slab = dispenser.Take(0);
            slab.Write("Test");

            IByteBuffer newBuffer = slab.Take();

            Assert.AreEqual((byte) 'T', newBuffer[0]);
            Assert.AreEqual((byte) 'e', newBuffer[1]);
            Assert.AreEqual((byte) 's', newBuffer[2]);
            Assert.AreEqual((byte) 't', newBuffer[3]);

            newBuffer.Start = 1;

            Assert.AreEqual((byte) 'e', newBuffer[0]);
            Assert.AreEqual((byte) 's', newBuffer[1]);
            Assert.AreEqual((byte) 't', newBuffer[2]);

            dispenser.Return(slab);
            dispenser.Return(slabUnused);
        }
    }
}