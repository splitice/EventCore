using System;
using System.Collections;
using System.Collections.Generic;

namespace EventCore.Memory
{
    internal class BytePointerEnumerator : IEnumerator<byte>
    {
        private int _length;
        private ArraySegment<byte> _ptr;
        private int _start;


        public BytePointerEnumerator(ArraySegment<byte> ptr, int start, int length)
        {
            _ptr = ptr;
            _ptr = new ArraySegment<byte>(ptr.Array, start + ptr.Offset, length - start);
            _length = length;
        }

        #region IEnumerator<byte> Members

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            _start++;
            _length--;
            return _length != -1;
        }

        public void Reset()
        {
            _length += _start;
            _start = 0;
        }

        public byte Current
        {
            get { return _ptr.Array[_ptr.Offset + _start]; }
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        #endregion
    }
}