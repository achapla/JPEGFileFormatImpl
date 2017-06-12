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
    internal class APP12
    {
        UInt16 length;
        string tag;
        UInt32 quality;
        string comment;
        string copyright;

        internal APP12(BinaryReaderFlexiEndian reader)
        {
            length = reader.ReadUInt16(); //Length of structure

            while (reader.PeekChar() != 0x00)
                tag += reader.ReadChar();

            UInt16 tagId = reader.ReadUInt16();

            while (tagId != 0)
            {
                UInt16 dataLen = reader.ReadUInt16();

                switch (tagId)
                {
                    case 1: //Quality
                        quality = reader.ReadUInt32();
                        break;
                    case 2: //Comment
                        comment = reader.ReadString();
                        break;
                    case 3: //Copyright
                        copyright = reader.ReadString();
                        break;
                    default:
                        break;
                }

                tagId = reader.ReadUInt16();
            }
        }
    }
}
