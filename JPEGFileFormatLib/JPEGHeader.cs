using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace JPEGFileFormatLib
{
    internal class JPEGHeader
    {
        UInt16 soi;
        UInt16 marker;
        UInt16 markerSize;
        UInt32 identifier;

        public bool IsJPEG { get { return soi == 0xFFD8 && (marker & 0xFFE0) == 0xFFE0; } }
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
            //decode https://www.impulseadventure.com/photo/jpeg-huffman-coding.html
            SOS lastScan = (SOS)objs.Last(obj => obj.GetType().Equals(typeof(SOS)));
            DHT lastDHT = (DHT)objs.Last(obj => obj.GetType().Equals(typeof(DHT)));
            lastDHT.GenerateBitArrayMap();

            using (MemoryStream cdms = new MemoryStream(lastScan.compressedData))
            {
                bool isEOF = false;
                StringBuilder binaryStringBuilder = new StringBuilder();
                List<MCU> mcus = new List<MCU>();
                byte lastIntervalValue = 0xD0;

                do
                {
                    byte[] buffer = new byte[cdms.Length - cdms.Position > 1024 ? 1024 : cdms.Length - cdms.Position];
                    buffer = new byte[1024];
                    cdms.Read(buffer, 0, buffer.Length);
                    isEOF = cdms.Position == cdms.Length;
                    byte? lastByte = null;
                    bool restartIntervalDetected = false;
                    foreach (var b in buffer)
                    {
                        if (lastByte == null)
                        {
                            lastByte = b;
                            continue;
                        }

                        if (lastByte == 0xFF && b == 0x00)
                        {
                            continue;
                        }
                        else if (lastByte == 0xFF && b == lastIntervalValue)
                        {
                            binaryStringBuilder.Append('|');
                            restartIntervalDetected = true;
                            lastByte = b;
                            lastIntervalValue++;
                            if (lastIntervalValue == 0xD8)
                                lastIntervalValue = 0xD0;
                            //lastByte = null;
                            continue;
                        }

                        if (restartIntervalDetected)
                        {
                            binaryStringBuilder.Append(Convert.ToString(b, 2));
                            restartIntervalDetected = false;
                            lastByte = null;
                            continue;
                        }
                        else
                            binaryStringBuilder.Append(Convert.ToString(lastByte.Value, 2).PadLeft(8, '0'));
                        lastByte = b;
                        //if (previousWasFF)
                        //{
                        //    if (b == 0x00)
                        //    {
                        //        previousWasFF = false;
                        //        continue;
                        //    }
                        //}
                        //previousWasFF = b == 0xFF;
                        //binaryStringBuilder.Append(Convert.ToString(b, 2).PadLeft(8, '0'));
                    }

                    if (lastByte.HasValue)
                        binaryStringBuilder.Append(Convert.ToString(lastByte.Value, 2).PadLeft(8, '0'));

                    try
                    {
                        while (binaryStringBuilder.Length > 4092)
                        {
                            if (binaryStringBuilder.ToString().Substring(0, 8).Contains('|'))
                            {
                                binaryStringBuilder.Remove(0, binaryStringBuilder.ToString().IndexOf('|') + 1);
                            }
                            if (mcus.Count == 1976)
                            {
                                Console.WriteLine();
                            }
                            MCU mCU = new MCU();
                            mCU.ReadData(lastDHT, binaryStringBuilder);
                            mcus.Insert(0, mCU);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

                    //decimal dcLuminance = GetLuminanceDC(lastDHT, binaryStringBuilder);
                    //decimal acLuminance = GetLuminanceAC(lastDHT, binaryStringBuilder);
                    //while (acLuminance != 0)
                    //{
                    //    acLuminance = GetLuminanceAC(lastDHT, binaryStringBuilder);
                    //}

                    //decimal dcChrominanceCb = GetChrominanceDC(lastDHT, binaryStringBuilder);
                    //decimal acChrominanceCb = GetChrominanceAC(lastDHT, binaryStringBuilder);
                    //while (acChrominanceCb != 0)
                    //{
                    //    acChrominanceCb = GetChrominanceAC(lastDHT, binaryStringBuilder);
                    //}

                    //decimal dcChrominanceCr = GetChrominanceDC(lastDHT, binaryStringBuilder);
                    //decimal acChrominanceCr = GetChrominanceAC(lastDHT, binaryStringBuilder);
                    //while (acChrominanceCr != 0)
                    //{
                    //    acChrominanceCr = GetChrominanceAC(lastDHT, binaryStringBuilder);
                    //}
                } while (!isEOF);
            }
        }

        private decimal GetChrominanceAC(DHT lastDHT, StringBuilder binaryStringBuilder)
        {
            DHT.DHTStruct acChrominanceTable = lastDHT.tables.First(t => t.TableType == DHT.HuffmanTableType.AC && t.numberOfHT == 1);

            int i = 0;
            while (!acChrominanceTable.bitMaps.ContainsKey(binaryStringBuilder.ToString().Substring(0, i)))
                i++;

            int additionalBitsToConvert = acChrominanceTable.bitMaps[binaryStringBuilder.ToString().Substring(0, i)];
            if (additionalBitsToConvert == 0)
            {
                binaryStringBuilder.Remove(0, i + additionalBitsToConvert);
                return 0;
            }
            string acCodeValue = binaryStringBuilder.ToString().Substring(i, additionalBitsToConvert);
            binaryStringBuilder.Remove(0, i + additionalBitsToConvert);
            if (acCodeValue.StartsWith("0"))
                return ~Convert.ToInt32(acCodeValue, 2);
            return Convert.ToInt32(acCodeValue, 2);
        }

        private decimal GetChrominanceDC(DHT lastDHT, StringBuilder binaryStringBuilder)
        {
            DHT.DHTStruct dcChrominanceTable = lastDHT.tables.First(t => t.TableType == DHT.HuffmanTableType.DC && t.numberOfHT == 1);

            int i = 0;
            while (!dcChrominanceTable.bitMaps.ContainsKey(binaryStringBuilder.ToString().Substring(0, i)))
                i++;

            int additionalBitsToConvert = dcChrominanceTable.bitMaps[binaryStringBuilder.ToString().Substring(0, i)];
            if (additionalBitsToConvert == 0)
            {
                binaryStringBuilder.Remove(0, i + additionalBitsToConvert);
                return 0;
            }
            string dcCodeValue = binaryStringBuilder.ToString().Substring(i, additionalBitsToConvert);
            binaryStringBuilder.Remove(0, i + additionalBitsToConvert);
            if (dcCodeValue.StartsWith("0"))
                return ~Convert.ToInt32(dcCodeValue, 2);
            return Convert.ToInt32(dcCodeValue, 2);
        }

        private decimal GetLuminanceAC(DHT lastDHT, StringBuilder binaryStringBuilder)
        {
            DHT.DHTStruct acLuminanceTable = lastDHT.tables.First(t => t.TableType == DHT.HuffmanTableType.AC && t.numberOfHT == 0);

            int i = 0;
            while (!acLuminanceTable.bitMaps.ContainsKey(binaryStringBuilder.ToString().Substring(0, i)))
                i++;

            int additionalBitsToConvert = acLuminanceTable.bitMaps[binaryStringBuilder.ToString().Substring(0, i)];
            if (additionalBitsToConvert == 0)
            {
                binaryStringBuilder.Remove(0, i + additionalBitsToConvert);
                return 0;
            }
            string acCodeValue = binaryStringBuilder.ToString().Substring(i, additionalBitsToConvert);
            binaryStringBuilder.Remove(0, i + additionalBitsToConvert);
            if (acCodeValue.StartsWith("0"))
                return ~Convert.ToInt32(acCodeValue, 2);
            return Convert.ToInt32(acCodeValue, 2);
        }

        private decimal GetLuminanceDC(DHT lastDHT, StringBuilder binaryStringBuilder)
        {
            DHT.DHTStruct dcLuminanceTable = lastDHT.tables.First(t => t.TableType == DHT.HuffmanTableType.DC && t.numberOfHT == 0);

            int i = 0;
            while (!dcLuminanceTable.bitMaps.ContainsKey(binaryStringBuilder.ToString().Substring(0, i)))
                i++;

            int additionalBitsToConvert = dcLuminanceTable.bitMaps[binaryStringBuilder.ToString().Substring(0, i)];
            if (additionalBitsToConvert == 0)
            {
                binaryStringBuilder.Remove(0, i + additionalBitsToConvert);
                return 0;
            }
            string dcCodeValue = binaryStringBuilder.ToString().Substring(i, additionalBitsToConvert);
            binaryStringBuilder.Remove(0, i + additionalBitsToConvert);
            if (dcCodeValue.StartsWith("0"))
                return ~Convert.ToInt32(dcCodeValue, 2);
            return Convert.ToInt32(dcCodeValue, 2);
        }
    }
}
