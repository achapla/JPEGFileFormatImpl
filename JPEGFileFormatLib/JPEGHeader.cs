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

        public bool IsJPEG { get { return soi == 0xffd8 && (marker & 0xffe0) == 0xffe0; } }
        public bool IsEXIF { get { return IsJPEG && identifier == 0x45786966; } }
        public bool IsJFIF { get { return IsJPEG && identifier == 0x4A464946; } }

        List<object> objs = new List<object>();

        internal void ReadHeader(BinaryReaderFlexiEndian reader)
        {
            Console.WriteLine("Reading header...");

            reader.UseBigEndian = true;
            reader.BaseStream.Position = 0;
            soi = reader.ReadUInt16();  // Start of Image (SOI) marker (FFD8)
            marker = reader.ReadUInt16(); // JFIF marker (FFE0) EXIF marker (FFE1)
            markerSize = reader.ReadUInt16(); // size of marker data (incl. marker)
            identifier = reader.ReadUInt32(); // JFIF 0x4649464a or Exif  0x66697845

            byte[] tagMarker = new byte[2];
            while (reader.PeekChar() == 0x00)
                tagMarker[0] = reader.ReadByte(); //Discard parity
            reader.BaseStream.Position = 2;
            ReadStructure(reader);
        }

        private void ReadStructure(BinaryReaderFlexiEndian reader)
        {
            Console.WriteLine("Reading structure...");
            while (reader.BaseStream.Position != reader.BaseStream.Length)
            {
                JFIFMarkers markerTag = (JFIFMarkers)reader.ReadUInt16();
                Console.WriteLine($"Current marker tag : {markerTag.ToString()}");
                switch (markerTag)
                {
                    case JFIFMarkers.APP0:
                        objs.Add(new APP0(reader));
                        break;
                    case JFIFMarkers.APP1:
                        objs.Add(new APP1(reader));
                        break;
                    case JFIFMarkers.APP2:
                        objs.Add(new APP2(reader));
                        break;
                    case JFIFMarkers.APP12:
                        objs.Add(new APP12(reader));
                        break;
                    case JFIFMarkers.APP13:
                        objs.Add(new APP13(reader));
                        break;
                    case JFIFMarkers.APP14:
                        objs.Add(new APP14(reader));
                        break;
                    case JFIFMarkers.DRI:
                        objs.Add(new DRI(reader));
                        break;
                    case JFIFMarkers.DQT:
                        objs.Add(new DQT(reader));
                        break;
                    case JFIFMarkers.DHT:
                        objs.Add(new DHT(reader));
                        break;
                    case JFIFMarkers.COM:
                        objs.Add(new COM(reader));
                        break;
                    case JFIFMarkers.SOS:
                        objs.Add(new SOS(reader));
                        break;
                    case JFIFMarkers.SOI:
                        break;
                    case JFIFMarkers.EOI:
                        break;
                    case JFIFMarkers.SOF0:
                        objs.Add(new SOF0(reader));
                        break;
                    default:
                        objs.Add(new QuantizationTable(reader));
                        Console.WriteLine("New tag : {0}", markerTag);
                        break;
                }
                //JPEGQuantizationTable dqt = new JPEGQuantizationTable(reader);
            }

            SOS lastScan = (SOS)objs.Last(obj => obj.GetType().Equals(typeof(SOS)));
            byte[,] data = new byte[8, 8];

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    data[i, j] = lastScan.compressedData[i * 8 + j];
                    Console.Write(data[i, j].ToString("X") + " ");
                }
                Console.WriteLine();
            }
        }
    }
}
