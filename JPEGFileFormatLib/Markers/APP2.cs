using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPEGFileFormatLib
{
    internal class APP2
    {
        long start;
        UInt16 length;
        string tag;
        byte blockNumber;
        byte blockTotal;
        byte[] data;

        internal APP2(BinaryReaderFlexiEndian reader)
        {
            start = reader.BaseStream.Position;
            length = reader.ReadUInt16();

            while (reader.PeekChar() != 0)
                tag += reader.ReadChar();

            byte extraByte = reader.ReadByte(); //Discard null terminator

            blockNumber = reader.ReadByte();
            blockTotal = reader.ReadByte();
            data = reader.ReadBytes((int)(length + start - reader.BaseStream.Position));
        }
    }
}
