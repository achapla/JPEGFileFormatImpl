using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPEGFileFormatLib
{
    internal class DQT
    {
        UInt16 length;
        List<DQTStruct> tables = new List<DQTStruct>();

        internal DQT(BinaryReaderFlexiEndian reader)
        {
            length = reader.ReadUInt16();

            for (int i = 0; i < (length - 2) / 65; i++)
                tables.Add(new DQTStruct(reader));
        }

        internal class DQTStruct
        {
            byte QTInformation;
            byte[] data;
            byte numberOfQT;
            byte precision { get { return (byte)(QTInformation & 0xF0); } }

            internal DQTStruct(BinaryReaderFlexiEndian reader)
            {
                QTInformation = reader.ReadByte();
                BitArray HTInformationBitArray = new BitArray(new byte[] { QTInformation });
                for (byte j = 0; j < 4; j++)
                {
                    if (HTInformationBitArray.Get(j))
                    {
                        numberOfQT = (byte)(j + 1);
                        break;
                    }
                }
                data = reader.ReadBytes(64 * (precision + 1));
            }
        }
    }
}
