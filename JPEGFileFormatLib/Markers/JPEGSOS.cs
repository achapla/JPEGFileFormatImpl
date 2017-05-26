using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPEGFileFormatLib
{
    internal class JPEGSOS
    {
        UInt16 length;
        byte numberOfComponentsInScan;
        public byte ComponentId;
        public byte HuffmanTableToUse;
        byte startOfSpectralSelection;
        byte endOfSpectralSelection;

        internal JPEGSOS(BinaryReaderFlexiEndian reader)
        {
            length = reader.ReadUInt16(); //(UInt16)(reader.ReadByte() * 256 + reader.ReadByte()); //Length of structure
            numberOfComponentsInScan = reader.ReadByte();
            //UInt16 eachComponent = reader.ReadUInt16();
            ComponentId = reader.ReadByte();
            HuffmanTableToUse = reader.ReadByte();

            //reader.ReadBytes(3); //Ignorable bytes
            startOfSpectralSelection = reader.ReadByte();
            endOfSpectralSelection = reader.ReadByte();
            reader.ReadByte();
        }
    }
}
