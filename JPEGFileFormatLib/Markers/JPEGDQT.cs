using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPEGFileFormatLib
{
    internal class JPEGDQT
    {
        UInt16 length;
        byte QTInformation;
        byte[] data;
        byte numberOfQT;
        byte precision { get { return (byte)(QTInformation & 0xF0); } }

        internal JPEGDQT(BinaryReaderFlexiEndian reader)
        {
            length = reader.ReadUInt16();
            QTInformation = reader.ReadByte();
            BitArray HTInformationBitArray = new BitArray(new byte[] { QTInformation });
            for (byte i = 0; i < 4; i++)
            {
                if (HTInformationBitArray.Get(i))
                {
                    numberOfQT = (byte)(i + 1);
                    break;
                }
            }
            data = reader.ReadBytes(64 * (precision + 1));
        }
    }
}
