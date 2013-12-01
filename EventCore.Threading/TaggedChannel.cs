using System.Collections.Generic;

namespace EventCore.Threading
{
    /// <summary>
    /// A channel with a tag
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class TaggedChannel<TKey, TValue> : Channel<KeyValuePair<TKey, TValue>>
    {
    }
}