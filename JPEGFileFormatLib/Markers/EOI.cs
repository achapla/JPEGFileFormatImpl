using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPEGFileFormatLib.Markers
{
    /// <summary>
    /// Indicates end of compress image data stream. There should not be any other marker for current image after this marker.
    /// </summary>
    internal class EOI : JpegMarkerBase
    {
        public EOI() : base(JpegMarker.EOI){}

        public override void Read(BinaryReaderFlexiEndian reader) { }

        public override void ReadExtensionData(BinaryReaderFlexiEndian reader) { }
    }
}

