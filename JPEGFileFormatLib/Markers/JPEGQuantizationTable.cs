using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPEGFileFormatLib
{
    internal class JPEGQuantizationTable
    {
        UInt16 length;
        byte[] dqt;

        internal JPEGQuantizationTable(BinaryReaderFlexiEndian reader)
        {
            length = reader.ReadUInt16(); //(UInt16)(reader.ReadByte() * 256 + reader.ReadByte()); //Length of structure
            dqt = reader.ReadBytes(length - 2);
        }
    }
}
