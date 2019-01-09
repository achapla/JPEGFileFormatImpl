using JPEGFileFormatLib.Markers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPEGFileFormatLib
{
    /// <summary>
    /// Minimum coded unit
    /// https://www.impulseadventure.com/photo/jpeg-minimum-coded-unit.html
    /// </summary>
    internal class OptimizedMCU
    {
        readonly Markers.DHT.DHTStruct DCLuminanceTable;
        readonly Markers.DHT.DHTStruct ACLuminanceTable;
        readonly Markers.DHT.DHTStruct DCChrominanceTable;
        readonly Markers.DHT.DHTStruct ACChrominanceTable;
        public Component[] Components;

        public OptimizedMCU(Markers.DHT.DHTStruct dcLuminanceTable, Markers.DHT.DHTStruct acLuminanceTable, Markers.DHT.DHTStruct dcChrominanceTable, Markers.DHT.DHTStruct acChrominanceTable)
        {
            DCLuminanceTable = dcLuminanceTable;
            ACLuminanceTable = acLuminanceTable;
            DCChrominanceTable = dcChrominanceTable;
            ACChrominanceTable = acChrominanceTable;
        }

        public void Read(BinaryReaderFlexiEndian reader)
        {
            Components = new Component[3] { new Component("Y"), new Component("Cb"), new Component("Cr") };
            Components[0].Read(reader, DCLuminanceTable, ACLuminanceTable);
            Components[1].Read(reader, DCChrominanceTable, ACChrominanceTable);
            Components[2].Read(reader, DCChrominanceTable, ACChrominanceTable);
        }

        public override string ToString()
        {
            return String.Join(", ", Components.Select(c => c.ChannelName + " = " + c.DCValue));
        }

        public class Component
        {
            public string ChannelName { get; private set; }
            public int DCValue { get; private set; }
            public int[] ACValues { get; } = new int[63];

            public Component(string channelName)
            {
                ChannelName = channelName;
            }

            public void Read(BinaryReaderFlexiEndian reader, Markers.DHT.DHTStruct dcTable, Markers.DHT.DHTStruct acTable)
            {
                ReadDCValue(reader, dcTable);
                ReadACValue(reader, acTable);
            }

            private void ReadACValue(BinaryReaderFlexiEndian reader, DHT.DHTStruct acTable)
            {
                for (int i = 0; i < ACValues.Length; i++)
                {
                    if (i == 28)
                        i = i;
                    reader.FillBitData();
                    int bitLength = acTable.MinCodeLength - 1;
                    ushort codeValue;

                    BitVector32 debugBits = new BitVector32((int)(reader.DataBuffer >> 32));

                    do
                    {
                        codeValue = reader.TakeBits(++bitLength);
                    } while (!acTable.CodeTable.ContainsKey(codeValue));

                    reader.RemoveBits(bitLength);

                    int dataLength = acTable.CodeTable[codeValue];

                    if (dataLength == 0)
                        break;

                    i += (dataLength / 16);
                    dataLength = dataLength % 16;

                    uint coefficientValue = reader.TakeBits(dataLength);
                    if ((reader.DataBuffer >> 63) == 0x0)
                    {
                        coefficientValue = (((~coefficientValue) << (64 - dataLength)) >> (64 - dataLength));
                        ACValues[i] = -(int)coefficientValue;
                    }
                    else
                        ACValues[i] = (int)coefficientValue;

                    reader.RemoveBits(dataLength);

                    if (ACValues.Length - 1 == i)
                        break;
                }
            }

            private void ReadDCValue(BinaryReaderFlexiEndian reader, Markers.DHT.DHTStruct dcTable)
            {
                reader.FillBitData();
                int bitLength = dcTable.MinCodeLength - 1;
                ushort codeValue;

                BitVector32 debugBits = new BitVector32((int)(reader.DataBuffer >> 32));

                do
                {
                    codeValue = reader.TakeBits(++bitLength);
                } while (!dcTable.CodeTable.ContainsKey(codeValue));

                reader.RemoveBits(bitLength);

                int dataLength = dcTable.CodeTable[codeValue];
                if (dataLength == 0)
                    return;

                uint coefficientValue = reader.TakeBits(dataLength);
                if ((reader.DataBuffer >> 63) == 0x0)
                {
                    coefficientValue = (((~coefficientValue) << (64 - dataLength)) >> (64 - dataLength));
                    DCValue = -(int)coefficientValue;
                }
                else
                    DCValue = (int)coefficientValue;
                reader.RemoveBits(dataLength);
            }
        }
    }
}
