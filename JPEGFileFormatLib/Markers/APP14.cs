using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPEGFileFormatLib
{
    /// <summary>
    /// FF EE
    /// </summary>
    internal class APP14
    {
        UInt16 length;
        string tag;
        byte version;
        UInt16 flag0;
        UInt16 flag1;
        byte transform;

        internal APP14(BinaryReaderFlexiEndian reader)
        {
            length = reader.ReadUInt16(); //Length of structure

            while (reader.PeekChar() != 0)
                tag += reader.ReadChar();

            reader.ReadByte(); //Discard null terminator

            version = reader.ReadByte();
            flag0 = reader.ReadUInt16();
            flag1 = reader.ReadUInt16();
            transform = reader.ReadByte();
        }
    }
}
