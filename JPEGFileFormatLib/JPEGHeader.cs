using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPEGFileFormatLib
{
    internal class JPEGHeader
    {
        UInt16 soi;
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

        public bool IsJPEG { get { return soi == 0xffd8 && (marker & 0xffe0) == 0xffe0; } }
        public bool IsEXIF { get { return IsJPEG && identifier == 0x45786966; } }
        public bool IsJFIF { get { return IsJPEG && identifier == 0x4A464946; } }

        List<object> objs = new List<object>();

        internal void ReadHeader(BinaryReaderFlexiEndian reader)
        {
            byte[] vals = reader.ReadBytes(40);

            reader.UseBigEndian = true;
            reader.BaseStream.Position = 0;
            soi = reader.ReadUInt16();  // Start of Image (SOI) marker (FFD8)
            marker = reader.ReadUInt16(); // JFIF marker (FFE0) EXIF marker (FFE1)
            markerSize = reader.ReadUInt16(); // size of marker data (incl. marker)
            identifier = reader.ReadUInt32(); // JFIF 0x4649464a or Exif  0x66697845
            reader.ReadByte(); //Discard parity
            versionMajor = reader.ReadByte(); //Version 1.02 is the current released revision
            versionMinor = reader.ReadByte(); //Version 1.02 is the current released revision
            units = reader.ReadByte(); //Units for X and Y densities => 0:no units(pixel) => 1:dots per inch => 2:dots per cm
            XDensity = reader.ReadUInt16(); //Horizontal pixel density
            YDensity = reader.ReadUInt16(); //Vertical pixel density
            XThumbnail = reader.ReadByte(); // Thumbnail horizontal pixel count
            YThumbnail = reader.ReadByte(); // Thumbnail vertical pixel count

            ReadRGBForThunbnail(reader);
            ReadStructure(reader);
        }

        private void ReadStructure(BinaryReaderFlexiEndian reader)
        {
            while (reader.BaseStream.Position != reader.BaseStream.Length)
            {
                JPEGJFIFMarkers markerTag = (JPEGJFIFMarkers)reader.ReadUInt16();
                switch (markerTag)
                {
                    case JPEGJFIFMarkers.APP1:
                        objs.Add(new JPEGAPP1(reader));
                        break;
                    case JPEGJFIFMarkers.APP12:
                        objs.Add(new JPEGAPP12(reader));
                        break;
                    case JPEGJFIFMarkers.APP14:
                        objs.Add(new JPEGAPP14(reader));
                        break;
                    case JPEGJFIFMarkers.DQT:
                        objs.Add(new JPEGDQT(reader));
                        break;
                    case JPEGJFIFMarkers.DHT:
                        objs.Add(new JPEGDHT(reader));
                        break;
                    case JPEGJFIFMarkers.COM:
                        objs.Add(new JPEGCOM(reader));
                        break;
                    case JPEGJFIFMarkers.SOS:
                        objs.Add(new JPEGSOS(reader));
                        break;
                    default:
                        objs.Add(new JPEGQuantizationTable(reader));
                        Console.WriteLine("New tag : {0}", markerTag);
                        break;
                }
                //JPEGQuantizationTable dqt = new JPEGQuantizationTable(reader);
            }
        }

        private void ReadRGBForThunbnail(BinaryReaderFlexiEndian reader)
        {
            int n = XThumbnail * YThumbnail;

            for (int i = 0; i < n; i++)
            {

            }
        }
    }
}
