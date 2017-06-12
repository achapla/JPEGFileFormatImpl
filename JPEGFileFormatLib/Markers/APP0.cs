using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPEGFileFormatLib
{
    /// <summary>
    /// FF E0
    /// </summary>
    internal class APP0
    {
        UInt16 marker;
        UInt16 markerSize;
        UInt32 identifier;
        Byte versionMajor;
        Byte versionMinor;
        Byte units;
        UInt16 XDensity;
        UInt16 YDensity;
        Byte XThumbnail;
        Byte YThumbnail;

        internal APP0(BinaryReaderFlexiEndian reader)
        {
            markerSize = reader.ReadUInt16(); // size of marker data (incl. marker)
            identifier = reader.ReadUInt32(); // JFIF 0x4649464a or Exif  0x66697845
            while (reader.PeekChar() == 0x00)
                reader.ReadByte(); //Discard parity
            versionMajor = reader.ReadByte(); //Version 1.02 is the current released revision
            versionMinor = reader.ReadByte(); //Version 1.02 is the current released revision
            units = reader.ReadByte(); //Units for X and Y densities => 0:no units(pixel) => 1:dots per inch => 2:dots per cm
            XDensity = reader.ReadUInt16(); //Horizontal pixel density
            YDensity = reader.ReadUInt16(); //Vertical pixel density
            XThumbnail = reader.ReadByte(); // Thumbnail horizontal pixel count
            YThumbnail = reader.ReadByte(); // Thumbnail vertical pixel count

            ReadRGBForThunbnail(reader);
        }

        private void ReadRGBForThunbnail(BinaryReaderFlexiEndian reader)
        {
            int n = XThumbnail * YThumbnail;

            Console.WriteLine($"Thumbnail size : {n}");
            for (int i = 0; i < n; i++)
            {

            }
        }
    }
}
