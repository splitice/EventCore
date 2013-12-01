using System.Collections.Generic;
using EventCore.Memory.Buffer;

namespace EventCore.Memory
{
    public class SlabDispenser
    {
        private readonly Stack<ByteBufferSlab> _buffers;
        private readonly SlabPool _pool;
        public int Length;

        public SlabDispenser(int slots, int size)
        {
            Length = size;
            _pool = new SlabPool(slots, size);
            _buffers = new Stack<ByteBufferSlab>();
            for (int i = 0; i < slots; i++)
            {
                _buffers.Push(new ByteBufferSlab(_pool.GetPointer(i), this));
            }
        }

        public ByteBufferSlab Take(int highmark = 5)
        {
            //return null;
            if (_buffers.Count <= highmark)
                return null;

            return _buffers.Pop();
        }

        public void Return(ByteBufferSlab buffer)
        {
            buffer.Clear();
            _buffers.Push(buffer);
        }
    }
}