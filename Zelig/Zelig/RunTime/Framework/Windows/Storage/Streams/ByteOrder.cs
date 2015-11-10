using System;
using Windows.Foundation;

namespace Windows.Storage.Streams
{
    /// <summary>Specifies the byte order of a stream.</summary>
    public enum ByteOrder
    {
        /// <summary>The least significant byte (lowest address) is stored first.</summary>
        LittleEndian,
        BigEndian,
    }
}
