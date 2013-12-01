using System.Collections.Generic;

namespace EventCore.Memory.Buffer
{
    public interface IByteList : IList<byte>
    {
        int Length { get; }
        byte[] ToByte();
    }
}