using System;
using System.Collections.Generic;
using System.Text;

namespace Snappy
{
    public enum SnappyFrameType : byte
    {
        Compressed = 0,
        Uncompressed = 1,
        UnskippableFirst = 2,
        UnskippableLast = 0x7f,
        SkippableFirst = 0x80,
        SkippableLast = 0xfd,
        Padding = 0xfe,
        StreamIdentifier = 0xff
    }
}
