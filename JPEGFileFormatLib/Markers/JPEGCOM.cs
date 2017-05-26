using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPEGFileFormatLib
{
    internal class JPEGCOM
    {
        UInt16 length;
        string comment;

        internal JPEGCOM(BinaryReaderFlexiEndian reader)
        {
            length = reader.ReadUInt16(); //Length of structure

            while (reader.PeekChar() != 0x00)
                comment += reader.ReadChar();

            reader.ReadByte(); //Discard null terminator
        }
    }
}
