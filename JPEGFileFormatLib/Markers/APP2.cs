using JPEGFileFormatLib.Markers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPEGFileFormatLib.Markers
{
    /// <summary>
    /// flashPix extension data. More data: https://sno.phy.queensu.ca/~phil/exiftool/TagNames/JPEG.html
    /// </summary>
    internal class APP2 : JpegMarkerBase
    {
        public string Tag { get; set; }
        public byte BlockNumber { get; set; }
        public byte BlockTotal { get; set; }
        public byte[] Data { get; set; }

        internal APP2() : base(JpegMarker.APP2)
        {
        }

        public override void ReadExtensionData(BinaryReaderFlexiEndian reader)
        {
            while (reader.PeekChar() != 0)
                Tag += reader.ReadChar();

            byte extraByte = reader.ReadByte(); //Discard null terminator

            BlockNumber = reader.ReadByte();
            BlockTotal = reader.ReadByte();
            //TODO Parse ICC Profiles, flashPix data
            Data = reader.ReadBytes((int)(MarkerSize + StartPosition - reader.BaseStream.Position));
        }
    }
}
