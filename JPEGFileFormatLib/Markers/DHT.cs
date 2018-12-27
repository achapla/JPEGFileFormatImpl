using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace JPEGFileFormatLib
{
    internal class DHT
    {
        readonly UInt16 length;
        public List<DHTStruct> tables = new List<DHTStruct>();

        internal DHT(BinaryReaderFlexiEndian reader)
        {
            length = reader.ReadUInt16();
            long endOfMarker = reader.BaseStream.Position + length - 2;
            while (reader.BaseStream.Position != endOfMarker)
                tables.Add(new DHTStruct(reader));
        }

        internal void GenerateBitArrayMap()
        {
            foreach (DHTStruct table in tables)
            {
                table.GenerateBitArrayMap();
            }
        }

        internal class DHTStruct
        {
            internal readonly byte HTInformation;
            internal byte[] data;
            internal byte numberOfHT { get { return (byte)(HTInformation & 0x0F); } }
            internal readonly byte typeOfHT; //type of HT, 0 = DC / Lossless Table, 1 = AC table
            internal HuffmanTableType TableType { get { return (HuffmanTableType)typeOfHT; } }
            internal byte[] numderOfSymbols;
            internal Dictionary<string, int> bitMaps = new Dictionary<string, int>();

            internal DHTStruct(BinaryReaderFlexiEndian reader)
            {
                HTInformation = reader.ReadByte();
                BitArray HTInformationBitArray = new BitArray(new byte[] { HTInformation });
                typeOfHT = (byte)(HTInformationBitArray.Get(4) ? 1 : 0);
                numderOfSymbols = reader.ReadBytes(16);
                var sumOfSymbols = numderOfSymbols.Sum(val => val);
                data = reader.ReadBytes(sumOfSymbols);
            }

            internal void GenerateBitArrayMap()
            {
                Dictionary<string, int> bitmap = new Dictionary<string, int>();
                int l = 0;

                foreach (var symbolItem in numderOfSymbols)
                {
                    if (l == data.Length)
                        break;
                    if (bitmap.Count == 0)
                    {
                        bitmap.Add("0", -1);
                        bitmap.Add("1", -1);
                    }
                    else
                    {
                        Dictionary<string, int> newbp = new Dictionary<string, int>();
                        foreach (var bmpk in bitmap.Keys)
                        {
                            newbp.Add(bmpk + "0", -1);
                            newbp.Add(bmpk + "1", -1);
                        }
                        bitmap = newbp;
                    }
                    for (int i = 0; i < symbolItem; i++)
                    {
                        KeyValuePair<string, int> firstOpen = bitmap.First();
                        bitmap.Remove(firstOpen.Key);
                        bitMaps.Add(firstOpen.Key, data[l++]);
                    }
                }
            }
        }

        internal enum HuffmanTableType
        {
            DC = 0,
            AC = 1
        }
    }
}
