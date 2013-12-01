namespace EventCore.Memory.Buffer
{
    internal class TrackedByteBufferArray : ByteBufferArray
    {
        public TrackedByteBufferArray()
        {
        }

        public TrackedByteBufferArray(int size)
            : base(size)
        {
        }

        ~TrackedByteBufferArray()
        {
            BufferManager.Instance.Return(this);
        }
    }
}