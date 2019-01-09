using JPEGFileFormatLib.Markers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPEGFileFormatLib.Markers
{
    /// <summary>
    /// Could be PictureInfo tags or Ducky tags. More info : https://sno.phy.queensu.ca/~phil/exiftool/TagNames/APP12.html
    /// </summary>
    internal class APP12 : JpegMarkerBase
    {
        public string Tag { get; set; }
        public uint Quality { get; set; }
        public string Comment { get; set; }
        public string Copyright { get; set; }

        internal APP12() : base(JpegMarker.APP12)
        {
        }

        public override void ReadExtensionData(BinaryReaderFlexiEndian reader)
        {
            while (reader.PeekChar() != 0x00)
                Tag += reader.ReadChar();

            ushort tagId = reader.ReadUInt16();

            while (tagId != 0)
            {
                ushort dataLen = reader.ReadUInt16();

                switch (tagId)
                {
                    case 1: //Quality
                        Quality = reader.ReadUInt32();
                        break;
                    case 2: //Comment
                        Comment = reader.ReadString();
                        break;
                    case 3: //Copyright
                        Copyright = reader.ReadString();
                        break;
                    default:
                        break;
                }

                tagId = reader.ReadUInt16();
            }
        }
    }
}
