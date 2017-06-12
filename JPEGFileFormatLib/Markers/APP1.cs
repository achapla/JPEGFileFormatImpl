using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPEGFileFormatLib
{
    /// <summary>
    /// FF E1
    /// </summary>
    internal class APP1
    {
        long start;
        UInt16 length;
        string tag;
        UInt16 tagMark;
        UInt32 firstIDFOffset;
        List<IFDBlock> blocks = new List<IFDBlock>();
        byte[] remain;
        bool isFirstApp1Marker { get { return tag.Equals("EXIF", StringComparison.OrdinalIgnoreCase); } }

        internal APP1(BinaryReaderFlexiEndian reader)
        {
            start = reader.BaseStream.Position;
            length = reader.ReadUInt16(); //Length of structure

            while (!(reader.PeekChar() == 0x00 || reader.PeekChar() == 0xFF))
                tag += reader.ReadChar();

            while (reader.PeekChar() == 0x00 || reader.PeekChar() == 0xFF)
                reader.ReadChar();

            if (isFirstApp1Marker)
            {
                long offsetMark = reader.BaseStream.Position;
                byte[] align = reader.ReadBytes(2);
                if (align[0] == 0x4D && align[1] == 0x4D)
                    reader.UseBigEndian = true;
                else
                    reader.UseBigEndian = false;

                tagMark = reader.ReadUInt16();

                if (tagMark != 42)
                    throw new NotSupportedException();

                UInt32 nextIFDOffset = firstIDFOffset = reader.ReadUInt32();
                while (nextIFDOffset != 0)
                {
                    IFDBlock ifdBlock = new IFDBlock(reader, nextIFDOffset, offsetMark);
                    blocks.Add(ifdBlock);
                    nextIFDOffset = ifdBlock.nextIDFOffset;
                }

                reader.UseBigEndian = true;
            }
            else
            {
                remain = reader.ReadBytes((int)(length - reader.BaseStream.Position + start));
            }
        }

        internal class IFDBlock
        {
            internal long start;
            internal UInt32 nextIDFOffset;
            internal DirectoryItem[] directoryItems;
            internal IFDBlock(BinaryReaderFlexiEndian reader, UInt32 IDFOffset, long offsetMark)
            {
                start = reader.BaseStream.Position;
                reader.BaseStream.Position = offsetMark + IDFOffset;

                UInt16 directoryLength = reader.ReadUInt16();
                directoryItems = new DirectoryItem[directoryLength];
                for (int i = 0; i < directoryLength; i++)
                {
                    directoryItems[i] = new DirectoryItem();
                    directoryItems[i].tagNumber = (ExifTag)reader.ReadUInt16();
                    directoryItems[i].dataFormat = reader.ReadUInt16();
                    directoryItems[i].numberOfComponents = reader.ReadUInt32();
                    if (directoryItems[i].DataLength > 4 || directoryItems[i].tagNumber == ExifTag.ExifOffset)
                        directoryItems[i].dataOffset = reader.ReadUInt32();
                    else
                    {
                        switch (directoryItems[i].DataLength)
                        {
                            case 1:
                                directoryItems[i].dataValue = reader.ReadByte();
                                reader.ReadBytes(3); //discard padding, if so
                                break;
                            case 2:
                                directoryItems[i].dataValue = reader.ReadUInt16();
                                reader.ReadUInt16(); //discard padding
                                break;
                            case 4:
                                directoryItems[i].dataValue = reader.ReadUInt32();
                                break;
                        }
                    }
                }

                nextIDFOffset = reader.ReadUInt32();

                foreach (var directoryItem in directoryItems)
                {
                    if (directoryItem.dataOffset != 0)
                    {
                        reader.BaseStream.Position = offsetMark + directoryItem.dataOffset;
                        if (directoryItem.tagNumber == ExifTag.ExifOffset)
                        {
                            UInt32 nextSubIFDOffset = directoryItem.dataOffset;
                            while (nextSubIFDOffset != 0)
                            {
                                IFDBlock ifdBlock = new IFDBlock(reader, nextSubIFDOffset, offsetMark);
                                directoryItem.blocks.Add(ifdBlock);
                                nextSubIFDOffset = ifdBlock.nextIDFOffset;
                            }
                        }
                        else
                        {
                            directoryItem.data = reader.ReadBytes((int)directoryItem.DataLength);
                        }

                        //while (reader.PeekChar() == 0x00)
                        //    reader.ReadByte();
                    }
                }
            }
        }
    }
}
