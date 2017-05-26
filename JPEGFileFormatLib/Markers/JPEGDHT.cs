using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPEGFileFormatLib
{
    internal class JPEGDHT
    {
        UInt16 length;
        byte HTInformation;
        byte[] data;
        byte numberOfHT;
        byte typeOfHT;
        byte[] numderOfSymbols;

        internal JPEGDHT(BinaryReaderFlexiEndian reader)
        {
            length = reader.ReadUInt16();
            HTInformation = reader.ReadByte();
            BitArray HTInformationBitArray = new BitArray(new byte[] { HTInformation });
            for (byte i = 0; i < 4; i++)
            {
                if (HTInformationBitArray.Get(i))
                {
                    numberOfHT = (byte)(i + 1);
                    break;
                }
            }
            typeOfHT = (byte)(HTInformationBitArray.Get(4) ? 1 : 0);
            numderOfSymbols = reader.ReadBytes(16);
            var sumOfSymbols = numderOfSymbols.Sum(val => val);
            data = reader.ReadBytes(sumOfSymbols);
        }
    }
}
