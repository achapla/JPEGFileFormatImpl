using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPEGFileFormatLib
{
    /// <summary>
    /// FF EC
    /// </summary>
    internal class JPEGAPP12
    {
        UInt16 length;
        string tag;
        UInt32 quality;
        string comment;
        string copyright;

        internal JPEGAPP12(BinaryReaderFlexiEndian reader)
        {
            byte[] vals = reader.ReadBytes(40);
            reader.BaseStream.Position -= 40;

            length = (UInt16)(reader.ReadByte() * 256 + reader.ReadByte()); //Length of structure
            vals = reader.ReadBytes(length - 2);
            int index = 0;

            while (vals[index] != 0x00)
                tag += (char)vals[index++];

            ++index; //Discard null terminator

            quality = BitConverter.ToUInt32(vals, index);
            index += 4;
        }
    }
}
