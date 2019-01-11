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
        public List<Component> Components = new List<Component>();

        public OptimizedMCU(SOF0 imageInfo, Markers.DHT.DHTStruct dcLuminanceTable, Markers.DHT.DHTStruct acLuminanceTable, Markers.DHT.DHTStruct dcChrominanceTable, Markers.DHT.DHTStruct acChrominanceTable)
        {
            DCLuminanceTable = dcLuminanceTable;
            ACLuminanceTable = acLuminanceTable;
            DCChrominanceTable = dcChrominanceTable;
            ACChrominanceTable = acChrominanceTable;

            foreach (SOF0.SOF0Component imageComp in imageInfo.Components)
            {
                if (imageComp.componentId == 1)
                {
                    if (imageComp.horizontal == 1)
                    {
                        Components.Add(new Component("Y", DCLuminanceTable, ACLuminanceTable));
                    }
                    else if (imageComp.horizontal == 2)
                    {
                        Components.Add(new Component("Y", DCLuminanceTable, ACLuminanceTable));
                        Components.Add(new Component("Y", DCLuminanceTable, ACLuminanceTable));
                        Components.Add(new Component("Y", DCLuminanceTable, ACLuminanceTable));
                        Components.Add(new Component("Y", DCLuminanceTable, ACLuminanceTable));
                    }
                }

                if (imageComp.componentId == 2)
                {
                    if (imageComp.vertical == 1)
                    {
                        Components.Add(new Component("Cb", DCChrominanceTable, ACChrominanceTable));
                    }
                }

                if (imageComp.componentId == 3)
                {
                    if (imageComp.vertical == 1)
                    {
                        Components.Add(new Component("Cr", DCChrominanceTable, ACChrominanceTable));
                    }
                }
            }
        }

        public void Read(BinaryReaderFlexiEndian reader)
        {
            foreach (var comp in Components)
            {
                comp.Read(reader);
            }
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
            public Markers.DHT.DHTStruct DCTable;
            public Markers.DHT.DHTStruct ACTable;

            public Component(string channelName, Markers.DHT.DHTStruct dcTable, Markers.DHT.DHTStruct acTable)
            {
                ChannelName = channelName;
                DCTable = dcTable;
                ACTable = acTable;
            }

            public void Read(BinaryReaderFlexiEndian reader)
            {
                ReadDCValue(reader);
                ReadACValue(reader);
            }

            private void ReadACValue(BinaryReaderFlexiEndian reader)
            {
                for (int i = 0; i < ACValues.Length; i++)
                {
                    if (i == 5)
                        i = i;
                    reader.FillBitData();
                    int bitLength = ACTable.MinCodeLength - 1;
                    ushort codeValue;

                    BitVector32 debugBits = new BitVector32((int)(reader.DataBuffer >> 32));

                    do
                    {
                        codeValue = reader.TakeBits(++bitLength);
                    } while (!(ACTable.CodeTable.ContainsKey(codeValue) && ACTable.CodeLength[codeValue] == bitLength));

                    reader.RemoveBits(bitLength);

                    int dataLength = ACTable.CodeTable[codeValue];

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

            private void ReadDCValue(BinaryReaderFlexiEndian reader)
            {
                reader.FillBitData();
                int bitLength = DCTable.MinCodeLength - 1;
                ushort codeValue;

                BitVector32 debugBits = new BitVector32((int)(reader.DataBuffer >> 32));

                do
                {
                    codeValue = reader.TakeBits(++bitLength);
                } while (!(DCTable.CodeTable.ContainsKey(codeValue) && DCTable.CodeLength[codeValue] == bitLength));

                reader.RemoveBits(bitLength);

                int dataLength = DCTable.CodeTable[codeValue];
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
