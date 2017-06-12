using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPEGFileFormatLib
{
    internal class APP13
    {
        long start;
        UInt16 length;
        string tag;
        byte AdobeCMType;
        List<APP13Block> blocks = new List<APP13Block>();

        internal APP13(BinaryReaderFlexiEndian reader)
        {
            start = reader.BaseStream.Position;
            length = reader.ReadUInt16();

            while (reader.PeekChar() != 0)
                tag += reader.ReadChar();

            reader.ReadByte(); //Discard null terminator

            if (tag.Equals("Adobe_CM", StringComparison.OrdinalIgnoreCase))
            {
                AdobeCMType = reader.ReadByte();
            }
            else
            {
                while (reader.BaseStream.Position != (start + length))
                    blocks.Add(new APP13Block(reader));
            }
        }

        internal class APP13Block
        {
            string tag;
            PhotoshopTag tagMarker;
            byte nameLength;
            byte padding;
            UInt32 size;
            byte[] data;
            internal APP13Block(BinaryReaderFlexiEndian reader)
            {
                for (int i = 0; i < 4; i++)
                    tag += reader.ReadChar();

                tagMarker = (PhotoshopTag)reader.ReadUInt16();
                nameLength = reader.ReadByte();
                padding = reader.ReadByte();
                size = reader.ReadUInt32();

                data = reader.ReadBytes((int)size);

                while (reader.PeekChar() == 0x00)
                    reader.ReadByte();
            }

            internal enum PhotoshopTag
            {
                PS_IPTCData = 0x0404,
                PS_JPEG_Quality = 0x0406,
                PS_PhotoshopBGRThumbnail = 0x0409,
                PS_CopyrightFlag = 0x040a,
                PS_URL = 0x040b,
                PS_PhotoshopThumbnail = 0x040c,
                PS_ICC_Profile = 0x040f,
                PS_GlobalAltitude = 0x0419,
                PS_EXIFInfo = 0x0422,
                PS_XMP = 0x0424,
                PS_IPTCDigest = 0x0425,
                PS_ClippingPathName = 0x0bb7,
            }
        }
    }
}
