using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPEGFileFormatLib
{
    internal class DHT
    {
        UInt16 length;
        List<DHTStruct> tables = new List<DHTStruct>();

        internal DHT(BinaryReaderFlexiEndian reader)
        {
            length = reader.ReadUInt16();
            long endOfMarker = reader.BaseStream.Position + length - 2;
            while (reader.BaseStream.Position != endOfMarker)
                tables.Add(new DHTStruct(reader));
        }

        internal class DHTStruct
        {
            byte HTInformation;
            byte[] data;
            byte numberOfHT { get { return (byte)(HTInformation & 0x0F); } }
            byte typeOfHT; //type of HT, 0 = DC table, 1 = AC table
            byte[] numderOfSymbols;

            internal DHTStruct(BinaryReaderFlexiEndian reader)
            {
                HTInformation = reader.ReadByte();
                BitArray HTInformationBitArray = new BitArray(new byte[] { HTInformation });
                typeOfHT = (byte)(HTInformationBitArray.Get(4) ? 1 : 0);
                numderOfSymbols = reader.ReadBytes(16);
                var sumOfSymbols = numderOfSymbols.Sum(val => val);
                data = reader.ReadBytes(sumOfSymbols);
            }
        }
    }
}
