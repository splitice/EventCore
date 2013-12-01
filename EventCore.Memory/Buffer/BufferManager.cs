namespace EventCore.Memory.Buffer
{
    public class BufferManager
    {
        private const int MaxBuffers = 1024;
        public static BufferManager Instance = new  BufferManager();
        private readonly SlabDispenser _manager;
        private int _allocated;

        public BufferManager()
        {
            _manager = new SlabDispenser(20000, 1024);
        }

        private int RequestedSize()
        {
            //For up to 40,000 connections
            if (_allocated < 20000)
            {
                return 512;
            }
            //For up to 70,000 connections
            if (_allocated < 30000)
            {
                return 256;
            }
            //More than 70,000 connections
            return 128;
        }

        public IByteBuffer Take()
        {
            IByteBuffer ret = _manager.Take();
            if (ret == null)
            {
                ret = new TrackedByteBufferArray(RequestedSize());
                _allocated++;
            }
            return ret;
        }

        public void Return(IByteBuffer byteBuffer)
        {
            if (byteBuffer is ByteBufferSlab)
            {
                _manager.Return(byteBuffer as ByteBufferSlab);
                return;
            }
            if (byteBuffer is TrackedByteBufferArray)
            {
                _allocated--;
            }
        }
    }
}