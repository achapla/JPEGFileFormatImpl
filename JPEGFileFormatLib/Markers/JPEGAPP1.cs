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
    internal class JPEGAPP1
    {
        UInt16 length;
        string tag;
        bool isBigEndian;
        DirectoryItem[] directoryItems;

        internal JPEGAPP1(BinaryReaderFlexiEndian reader)
        {
            byte[] vals = reader.ReadBytes(40);
            reader.BaseStream.Position -= 40;

            //reader.UseBigEndian = true;
            length = (UInt16)(reader.ReadByte() * 256 + reader.ReadByte()); //Length of structure
            //length = reader.ReadUInt16(); //Length of structure
            vals = reader.ReadBytes(length - 2);
            int index = 0;

            while (!(vals[index] == 0x00 || vals[index] == 0xFF))
                tag += (char)vals[index++];

            if (vals[index] == 0x00)
                index += 2; //Discard null terminator

            if (vals[index++] == 0x4D && vals[index++] == 0x4D)
                reader.UseBigEndian = isBigEndian = true;

            UInt16 reserve;
            if (isBigEndian)
                reserve = BitConverter.ToUInt16(vals.Reverse(2, index), 0);
            else
                reserve = BitConverter.ToUInt16(vals, index);
            index += 2;

            //if (reserve != 42)
            //    throw new NotSupportedException();

            UInt32 TagIFD0;
            if (isBigEndian)
                TagIFD0 = BitConverter.ToUInt32(vals.Reverse(4, index), 0);
            else
                TagIFD0 = BitConverter.ToUInt32(vals, index);
            index += 4;

            UInt16 directoryLength;
            if (isBigEndian)
                directoryLength = BitConverter.ToUInt16(vals.Reverse(2, index), 0);
            else
                directoryLength = BitConverter.ToUInt16(vals, index);
            index += 2;

            directoryItems = new DirectoryItem[directoryLength];
            for (int i = 0; i < directoryLength; i++)
            {
                directoryItems[i] = new DirectoryItem();

                if (isBigEndian)
                    directoryItems[i].tagNumber = (ExifTag)BitConverter.ToUInt16(vals.Reverse(2, index), 0);
                else
                    directoryItems[i].tagNumber = (ExifTag)BitConverter.ToUInt16(vals, index);
                index += 2;

                if (isBigEndian)
                    directoryItems[i].dataFormat = BitConverter.ToUInt16(vals.Reverse(2, index), 0);
                else
                    directoryItems[i].dataFormat = BitConverter.ToUInt16(vals, index);
                index += 2;

                if (isBigEndian)
                    directoryItems[i].numberOfComponents = BitConverter.ToUInt32(vals.Reverse(4, index), 0);
                else
                    directoryItems[i].numberOfComponents = BitConverter.ToUInt32(vals, index);
                index += 4;

                if (directoryItems[i].DataLength > 4)
                {
                    if (isBigEndian)
                        directoryItems[i].dataOffset = BitConverter.ToUInt32(vals.Reverse(4, index), 0);
                    else
                        directoryItems[i].dataOffset = BitConverter.ToUInt32(vals, index);
                    index += 4;
                }
                else
                {
                    switch (directoryItems[i].DataLength)
                    {
                        case 1:
                            directoryItems[i].dataValue = vals[1];
                            break;
                        case 2:
                            if (isBigEndian)
                                directoryItems[i].dataValue = BitConverter.ToUInt16(vals.Reverse(2, index), 0);
                            else
                                directoryItems[i].dataValue = BitConverter.ToUInt16(vals, index);
                            break;
                        case 4:
                            if (isBigEndian)
                                directoryItems[i].dataValue = BitConverter.ToUInt32(vals.Reverse(4, index), 0);
                            else
                                directoryItems[i].dataValue = BitConverter.ToUInt32(vals, index);
                            break;
                    }
                    index += 4;
                }
            }
        }
    }
}
