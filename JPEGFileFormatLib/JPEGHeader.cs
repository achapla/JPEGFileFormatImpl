using JPEGFileFormatLib.Markers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace JPEGFileFormatLib
{
    internal class JPEGHeader
    {
        //readonly ushort soi;
        //readonly ushort marker;
        //readonly ushort markerSize;
        //readonly uint identifier;

        //public bool IsJPEG { get { return soi == 0xFFD8 && (marker & 0xFFE0) == 0xFFE0; } }
        //public bool IsEXIF { get { return IsJPEG && identifier == 0x45786966; } }
        //public bool IsJFIF { get { return IsJPEG && identifier == 0x4A464946; } }

        readonly List<IJpegMarker> Markers = new List<IJpegMarker>();

        internal void ReadHeader(BinaryReaderFlexiEndian reader)
        {
            Console.WriteLine("Reading header...");

            //reader.UseBigEndian = true; //Data in jpeg files will always be in big endian format. Although EXIF marker can have little endian data for that particular segment only.
            ////TODO: Remove below part to standardize code
            //reader.BaseStream.Position = 0;
            //soi = reader.ReadUInt16();  // Start of Image (SOI) marker (FFD8)
            //marker = reader.ReadUInt16(); // JFIF marker (FFE0) EXIF marker (FFE1)
            //markerSize = reader.ReadUInt16(); // size of marker data (incl. marker)
            //identifier = reader.ReadUInt32(); // JFIF 0x4649464a or Exif  0x66697845

            //byte[] tagMarker = new byte[2];
            //while (reader.PeekChar() == 0x00)
            //    tagMarker[0] = reader.ReadByte(); //Discard parity
            //reader.BaseStream.Position = 0;
            ReadStructure(reader);
            DecodeOptimized(reader);
            //DecodeData();
        }

        private void ReadStructure(BinaryReaderFlexiEndian reader)
        {
            Console.WriteLine("Reading structure...");
            while (reader.BaseStream.Position != reader.BaseStream.Length)
            {
                JpegMarker markerTag = (JpegMarker)reader.ReadUInt16();
                Console.WriteLine($"Current marker tag : {markerTag.ToString()}\t{reader.BaseStream.Position}");
                IJpegMarker currentMarker = null;
                switch (markerTag)
                {
                    case JpegMarker.APP0:
                        currentMarker = new APP0();
                        break;
                    case JpegMarker.APP1:
                        currentMarker = new APP1();
                        break;
                    case JpegMarker.APP2:
                        currentMarker = new APP2();
                        break;
                    case JpegMarker.APP12:
                        currentMarker = new APP12();
                        break;
                    case JpegMarker.APP13:
                        currentMarker = new APP13();
                        break;
                    case JpegMarker.APP14:
                        currentMarker = new APP14();
                        break;
                    case JpegMarker.DRI:
                        currentMarker = new DRI();
                        break;
                    case JpegMarker.DQT:
                        currentMarker = new DQT();
                        break;
                    case JpegMarker.DHT:
                        currentMarker = new DHT();
                        break;
                    case JpegMarker.COM:
                        currentMarker = new COM();
                        break;
                    case JpegMarker.SOS:
                        currentMarker = new SOS();
                        break;
                    case JpegMarker.SOI:
                        currentMarker = new SOI();
                        break;
                    case JpegMarker.EOI:
                        currentMarker = new EOI();
                        break;
                    case JpegMarker.SOF0:
                        currentMarker = new SOF0();
                        break;
                    default:
                        currentMarker = new UnknownMarker(markerTag);
                        Console.WriteLine("================================New tag : {0}", markerTag);
                        break;
                }
                if (currentMarker != null)
                    currentMarker.Read(reader);
                Markers.Add(currentMarker);

                if (currentMarker is SOS) //Break when compressed data starts
                    break;
            }
        }

        private void DecodeOptimized(BinaryReaderFlexiEndian reader)
        {
            //if (Markers.OfType<APP1>().Count() == 0)
            //    return;

            List<DHT.DHTStruct> huffmanTables = Markers.OfType<DHT>().SelectMany(d => d.Tables).ToList();
            DHT.DHTStruct dcLuminanceTable = huffmanTables.FirstOrDefault(ht => ht.NumberOfHuffmanTable == 0 && ht.TableType == DHT.HuffmanTableType.DC);
            DHT.DHTStruct acLuminanceTable = huffmanTables.FirstOrDefault(ht => ht.NumberOfHuffmanTable == 0 && ht.TableType == DHT.HuffmanTableType.AC);
            DHT.DHTStruct dcChrominanceTable = huffmanTables.FirstOrDefault(ht => ht.NumberOfHuffmanTable == 1 && ht.TableType == DHT.HuffmanTableType.DC);
            DHT.DHTStruct acChrominanceTable = huffmanTables.FirstOrDefault(ht => ht.NumberOfHuffmanTable == 1 && ht.TableType == DHT.HuffmanTableType.AC);
            DRI restartInterval = Markers.OfType<DRI>().FirstOrDefault();
            SOF0 imageInfo = Markers.OfType<SOF0>().First();

            List<OptimizedMCU> mCUs = new List<OptimizedMCU>();

            DateTime st = DateTime.Now;

            while (reader.BaseStream.Position != reader.BaseStream.Length || !reader.IsEOF())
            {
                //if (mCUs.Count == 8281)
                //    Debugger.Break();

                //TODO Check for last blocks when reader ends
                OptimizedMCU optimizedMCU = new OptimizedMCU(imageInfo, dcLuminanceTable, acLuminanceTable, dcChrominanceTable, acChrominanceTable);
                optimizedMCU.Read(reader);
                mCUs.Insert(0, optimizedMCU);
                //mCUs.Add(optimizedMCU);

                if (restartInterval != null && mCUs.Count % restartInterval.RestartInterval == 0 && !reader.IsEOF())
                {
                    //Console.WriteLine((DateTime.Now - st).TotalMilliseconds);
                    //st = DateTime.Now;
                    restartInterval.ProcessCurrent(reader);
                }
            }
            Console.WriteLine((DateTime.Now - st).TotalMilliseconds);
        }

        private void DecodeData()
        {
            //decode https://www.impulseadventure.com/photo/jpeg-huffman-coding.html
            SOS lastScan = (SOS)Markers.Last(obj => obj.GetType().Equals(typeof(SOS)));
            DHT lastDHT = (DHT)Markers.Last(obj => obj.GetType().Equals(typeof(DHT)));

            using (MemoryStream cdms = new MemoryStream(lastScan.CompressedData))
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
            DHT.DHTStruct acChrominanceTable = lastDHT.Tables.First(t => t.TableType == DHT.HuffmanTableType.AC && t.NumberOfHuffmanTable == 1);

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
            DHT.DHTStruct dcChrominanceTable = lastDHT.Tables.First(t => t.TableType == DHT.HuffmanTableType.DC && t.NumberOfHuffmanTable == 1);

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
            DHT.DHTStruct acLuminanceTable = lastDHT.Tables.First(t => t.TableType == DHT.HuffmanTableType.AC && t.NumberOfHuffmanTable == 0);

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
            DHT.DHTStruct dcLuminanceTable = lastDHT.Tables.First(t => t.TableType == DHT.HuffmanTableType.DC && t.NumberOfHuffmanTable == 0);

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
