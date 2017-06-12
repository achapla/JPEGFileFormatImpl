using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPEGFileFormatLib
{
    /// <summary>
    /// FF DA
    /// </summary>
    internal class SOS
    {
        UInt16 length;
        byte numberOfComponentsInScan;
        List<SOSComponent> components = new List<SOSComponent>();
        byte startOfSpectralSelection;
        byte endOfSpectralSelection;
        byte approximationData;
        byte approximationHigh { get { return (byte)(approximationData & 0x0F); } }
        byte approximationLow { get { return (byte)(approximationData >> 4); } }
        byte[] compressedData;

        internal SOS(BinaryReaderFlexiEndian reader)
        {
            length = reader.ReadUInt16(); //(UInt16)(reader.ReadByte() * 256 + reader.ReadByte()); //Length of structure
            numberOfComponentsInScan = reader.ReadByte();

            for (int i = 0; i < numberOfComponentsInScan; i++)
                components.Add(new SOSComponent(reader));

            //reader.ReadBytes(3); //Ignorable bytes
            startOfSpectralSelection = reader.ReadByte();
            endOfSpectralSelection = reader.ReadByte();
            approximationData = reader.ReadByte();

            long dataStartOffset = reader.BaseStream.Position;
            bool eoiFound = false;
            int compressedDataLength = 1;

            //while (!eoiFound)
            //{
            //    var d = reader.ReadByte();
            //    if (d == 0xFF && reader.PeekChar() == 0xD9)
            //    {
            //        eoiFound = true;
            //        compressedDataLength = (int)(reader.BaseStream.Position - dataStartOffset);
            //    }
            //}

            while (!eoiFound)
            {
                byte[] subData = reader.ReadBytes(4096);

                for (int i = 0; i < subData.Length - 1; i++)
                {
                    if (subData[i] == 0xFF && subData[i + 1] == 0xD9)
                    {
                        eoiFound = true;
                        compressedDataLength += i - 1;
                        break;
                    }
                }

                if (!eoiFound)
                    compressedDataLength += subData.Length;
            }

            reader.BaseStream.Position = dataStartOffset;
            compressedData = reader.ReadBytes(compressedDataLength);
        }

        internal class SOSComponent
        {
            byte componentId;
            byte huffmanTableToUse;

            internal SOSComponent(BinaryReaderFlexiEndian reader)
            {
                componentId = reader.ReadByte();
                huffmanTableToUse = reader.ReadByte();
            }
        }
    }
}
