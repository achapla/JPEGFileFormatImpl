using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace JPEGFileFormatLib.Markers
{
    internal class DHT : JpegMarkerBase
    {
        public List<DHTStruct> Tables { get; set; } = new List<DHTStruct>();

        internal DHT() : base(JpegMarker.DHT)
        {
        }

        public override void ReadExtensionData(BinaryReaderFlexiEndian reader)
        {
            while (reader.BaseStream.Position != reader.BaseStream.Length)
                Tables.Add(new DHTStruct(reader));
        }

        internal class DHTStruct
        {
            /// <summary>
            /// bit 0..3 : number of HT (0..3, otherwise error)
            /// bit 4    : type of HT, 0 = DC table, 1 = AC table
            /// bit 5..7 : not used, must be 0
            /// </summary>
            internal byte HuffmanTableInformation { get; set; }
            /// <summary>
            /// These are real values to map with huffman code.
            /// </summary>
            internal byte[] DataCodes { get; set; }
            /// <summary>
            /// Indicates number of current huffman table.
            /// </summary>
            internal byte NumberOfHuffmanTable { get { return (byte)(HuffmanTableInformation & 0x0F); } }
            /// <summary>
            /// Identifies type of huffman table.
            /// </summary>
            internal HuffmanTableType TableType { get { return ((HuffmanTableInformation >> 4) & 0x1) == 0x1 ? HuffmanTableType.AC : HuffmanTableType.DC; } }
            /// <summary>
            /// Defines that how many symbols are present for each bit(index) array.
            /// </summary>
            internal byte[] NumderOfSymbolsOnEachBitLength { get; set; }
            internal Dictionary<string, int> bitMaps = new Dictionary<string, int>();
            /// <summary>
            /// Maps huffman code with real value from DataCodes array.
            /// </summary>
            internal Dictionary<ushort, int> CodeTable { get; set; }
            internal Dictionary<ushort, int> CodeLength { get; set; }
            internal int MinCodeLength { get; private set; }

            internal DHTStruct(BinaryReaderFlexiEndian reader)
            {
                HuffmanTableInformation = reader.ReadByte();
                NumderOfSymbolsOnEachBitLength = reader.ReadBytes(16);
                DataCodes = reader.ReadBytes(NumderOfSymbolsOnEachBitLength.Sum(val => val));
                CodeTable = new Dictionary<ushort, int>(DataCodes.Length);
                CodeLength = new Dictionary<ushort, int>(DataCodes.Length);
                GenerateBitArrayMap();
                GenerateOptimizedTable();
            }

            internal void GenerateOptimizedTable()
            {
                Queue<ushort> binaryTreeQueue = new Queue<ushort>();
                int dataCodeIndex = 0;
                bool minCodeLengthFound = false;
                int currentCodeLength = 0;

                foreach (var symbolItem in NumderOfSymbolsOnEachBitLength)
                {
                    ++currentCodeLength;

                    if (dataCodeIndex == DataCodes.Length) //If no more symbols break
                        break;

                    if (binaryTreeQueue.Count == 0)
                    {
                        if (!minCodeLengthFound)
                            ++MinCodeLength;
                        binaryTreeQueue.Enqueue(0);
                        binaryTreeQueue.Enqueue(1);
                    }
                    else
                    {
                        if (!minCodeLengthFound)
                            ++MinCodeLength;
                        int nextQueueLength = binaryTreeQueue.Count * 2; //We can expect to double queue size
                        while (nextQueueLength > binaryTreeQueue.Count)
                        {
                            ushort code = binaryTreeQueue.Dequeue();
                            binaryTreeQueue.Enqueue((ushort)(code << 1));
                            binaryTreeQueue.Enqueue((ushort)((code << 1) | 0x1));
                        }
                    }

                    //Now assign codes to respective values
                    for (int i = 0; i < symbolItem; i++)
                    {
                        minCodeLengthFound = true;
                        ushort codeValue = binaryTreeQueue.Dequeue();
                        CodeTable.Add(codeValue, DataCodes[dataCodeIndex++]);
                        CodeLength.Add(codeValue, currentCodeLength);
                    }
                }
            }

            internal void GenerateBitArrayMap()
            {
                Dictionary<string, int> bitmap = new Dictionary<string, int>();
                int l = 0;

                foreach (var symbolItem in NumderOfSymbolsOnEachBitLength)
                {
                    if (l == DataCodes.Length)
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
                        bitMaps.Add(firstOpen.Key, DataCodes[l++]);
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
