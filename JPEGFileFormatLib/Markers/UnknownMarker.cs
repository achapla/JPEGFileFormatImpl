using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPEGFileFormatLib.Markers
{
    internal class UnknownMarker : JpegMarkerBase
    {
        public byte[] Data { get; set; }

        public UnknownMarker(JpegMarker marker) : base(marker)
        {
        }

        public override void ReadExtensionData(BinaryReaderFlexiEndian reader)
        {
            Data = reader.ReadBytes(MarkerSize - 2);
        }
    }
}
