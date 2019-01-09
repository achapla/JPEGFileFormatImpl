using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPEGFileFormatLib.Markers
{
    internal interface IJpegMarker
    {
        long StartPosition { get; }
        JpegMarker Marker { get; }
        ushort MarkerSize { get; }
        void Read(BinaryReaderFlexiEndian reader);
    }
}
