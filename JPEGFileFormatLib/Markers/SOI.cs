using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPEGFileFormatLib.Markers
{
    /// <summary>
    /// Start of image marker. This marker does not contain any data so just skip reading anything.
    /// </summary>
    internal class SOI : JpegMarkerBase
    {
        public SOI() : base(JpegMarker.SOI) { }

        public override void Read(BinaryReaderFlexiEndian reader) { }

        public override void ReadExtensionData(BinaryReaderFlexiEndian reader) { }
    }
}
