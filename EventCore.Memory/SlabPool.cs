using System;

namespace EventCore.Memory
{
    internal class SlabPool
    {
        private readonly byte[] _memory;
        private readonly int _size;
        private readonly int _slots;

        public SlabPool(int slots, int size)
        {
            _memory = new byte[slots*size];
            _slots = slots;
            _size = size;
        }

        public ArraySegment<byte> GetPointer(int slot)
        {
            if (slot >= _slots)
            {
                throw new IndexOutOfRangeException();
            }

            return new ArraySegment<byte>(_memory, slot*_size, _size);
        }
    }
}