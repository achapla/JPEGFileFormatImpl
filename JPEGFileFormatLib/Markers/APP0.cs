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
    /// JFIF Format
    /// </summary>
    internal class APP0 : JpegMarkerBase
    {
        /// <summary>
        /// This zero terminated string (“JFIF”) uniquely identifies this APP0 marker. This string shall have zero parity (bit 7=0)
        /// </summary>
        public uint FormatIdentifier { get; set; }
        /// <summary>
        /// The most significant byte is used for major revisions, the least significant byte for minor revisions. Version 1.02 is the current released revision.
        /// </summary>
        public byte VersionMajor { get; set; }
        public byte VersionMinor { get; set; }
        /// <summary>
        /// Units for the X and Y densities. units = 0: no units, X and Y specify the pixel aspect ratio
        /// units = 1: X and Y are dots per inch
        /// units = 2: X and Y are dots per cm
        /// </summary>
        public byte Units { get; set; }
        /// <summary>
        /// Horizontal pixel density
        /// </summary>
        public ushort XDensity { get; set; }
        /// <summary>
        /// Vertical pixel density
        /// </summary>
        public ushort YDensity { get; set; }
        /// <summary>
        /// Thumbnail horizontal pixel count
        /// </summary>
        public byte XThumbnail { get; set; }
        /// <summary>
        /// Thumbnail vertical pixel count
        /// </summary>
        public byte YThumbnail { get; set; }

        internal APP0() : base(JpegMarker.APP0)
        {
        }

        public override void ReadExtensionData(BinaryReaderFlexiEndian reader)
        {
            FormatIdentifier = reader.ReadUInt32(); // JFIF 0x4649464a or Exif  0x66697845, should be JFIF in APP0 case and EXIF in APP1

            while (reader.PeekChar() == 0x00)
                reader.ReadByte(); //Discard parity

            VersionMajor = reader.ReadByte(); //Version 1.02 is the current released revision
            VersionMinor = reader.ReadByte(); //Version 1.02 is the current released revision
            Units = reader.ReadByte(); //Units for X and Y densities => 0:no units(pixel) => 1:dots per inch => 2:dots per cm
            XDensity = reader.ReadUInt16(); //Horizontal pixel density
            YDensity = reader.ReadUInt16(); //Vertical pixel density
            XThumbnail = reader.ReadByte(); // Thumbnail horizontal pixel count
            YThumbnail = reader.ReadByte(); // Thumbnail vertical pixel count

            ReadRGBForThunbnail(reader);
        }

        /// <summary>
        /// Packed (24-bit) RGB values for the thumbnail pixels, n = Xthumbnail* Ythumbnail
        /// </summary>
        /// <param name="reader"></param>
        private void ReadRGBForThunbnail(BinaryReader reader)
        {
            //TODO Find such file then parse and store thumbnail part
            //https://en.wikipedia.org/wiki/JPEG_File_Interchange_Format contains detailed format specification
            int n = XThumbnail * YThumbnail;

            Console.WriteLine($"Thumbnail size : {n}");
            for (int i = 0; i < n; i++)
            {

            }
        }
    }
}
