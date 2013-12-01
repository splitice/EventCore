using System;

namespace EventCore.Memory.Buffer
{
    public interface IByteBuffer : IByteList
    {
        bool Done { get; }
        int Start { get; set; }
        int End { get; set; }

        /// <summary>
        /// Swaps the "Data" with another new buffer (which is returned) without copy.
        /// </summary>
        /// <returns>the new buffer</returns>
        IByteBuffer Take();

        ArraySegment<byte> ToSegment(bool useRemaining);
        void Write(string str);
    }
}