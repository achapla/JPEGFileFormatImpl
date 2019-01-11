using JPEGFileFormatLib.Markers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPEGFileFormatLib.Markers
{
    /// <summary>
    /// EXIF format : https://www.media.mit.edu/pia/Research/deepview/exif.html
    /// </summary>
    internal class APP1 : JpegMarkerBase
    {
        //TODO Add more comments
        public string Tag { get; set; }
        public ushort TagMark { get; set; }
        public List<IFDBlock> Blocks { get; set; } = new List<IFDBlock>();
        public byte[] Remain { get; set; }
        public bool IsFirstApp1Marker { get { return Tag.Equals("EXIF", StringComparison.OrdinalIgnoreCase); } }

        internal APP1() : base(JpegMarker.APP1)
        {
        }

        public override void ReadExtensionData(BinaryReaderFlexiEndian reader)
        {
            while (!(reader.PeekChar() == 0x00 || reader.PeekChar() == 0xFF))
                Tag += reader.ReadChar();

            byte extraBytes;
            while (reader.PeekChar() == 0x00 || reader.PeekChar() == 0xFF)
                extraBytes = reader.ReadByte();

            if (IsFirstApp1Marker) //Structure of TIFF header
            {
                ushort align = reader.ReadUInt16();
                if (align == 0x4D4D) // 0x4949 = 'II' refers to 'intel' type(little endian) byte align and 0x4D4D = 'MM' for 'motorola' type(big endian) byte align
                    reader.UseBigEndian = true;
                else
                    reader.UseBigEndian = false;

                TagMark = reader.ReadUInt16();

                if (TagMark != 42)
                    throw new NotSupportedException();

                uint nextIFDOffset = reader.ReadUInt32();
                while (nextIFDOffset != 0)
                {
                    IFDBlock ifdBlock = new IFDBlock(nextIFDOffset, 6);
                    ifdBlock.Read(reader);
                    Blocks.Add(ifdBlock);
                    nextIFDOffset = ifdBlock.NextIFDOffset;
                }

                //Follows to thumbnail data
                reader.UseBigEndian = true;

                //TODO Parse thumbnail images
                //JPEGHeader thumbnailImage = new JPEGHeader();
                //thumbnailImage.ReadHeader(reader);

            }
            else
            {
                Console.WriteLine("//TODO: Read this data");
                Remain = reader.ReadBytes((int)(MarkerSize - reader.BaseStream.Position + StartPosition));
            }
        }

        /// <summary>
        /// Image file directory block
        /// directory length   | directory entry
        ///     2 byte         |    12 byte
        /// </summary>
        internal class IFDBlock
        {
            internal uint HeaderStartOffset { get; set; }
            internal uint CurrentIFDOffset { get; set; }
            internal uint NextIFDOffset { get; set; }
            internal DirectoryItem[] DirectoryItems { get; set; }

            public IFDBlock(uint currentIFDOffset, uint headerStartOffset)
            {
                CurrentIFDOffset = currentIFDOffset;
                HeaderStartOffset = headerStartOffset;
            }

            internal void Read(BinaryReaderFlexiEndian reader)
            {
                reader.BaseStream.Position = CurrentIFDOffset + HeaderStartOffset;
                ushort directoryLength = reader.ReadUInt16();
                DirectoryItems = new DirectoryItem[directoryLength];
                for (int i = 0; i < directoryLength; i++)
                {
                    DirectoryItems[i] = new DirectoryItem
                    {
                        TagNumber = (ExifTag)reader.ReadUInt16(),
                        DataFormat = (DirectoryItemDataFormat)reader.ReadUInt16(),
                        NumberOfComponents = reader.ReadUInt32()
                    };
                    if (DirectoryItems[i].DataLength > 4 || DirectoryItems[i].TagNumber == ExifTag.ExifOffset) //value contains offset to data stored address
                        DirectoryItems[i].DataOffset = reader.ReadUInt32();
                    else
                    {
                        switch (DirectoryItems[i].DataLength)
                        {
                            case 1:
                                DirectoryItems[i].DataValue = reader.ReadByte();
                                reader.ReadBytes(3); //discard padding, if so
                                break;
                            case 2:
                                DirectoryItems[i].DataValue = reader.ReadUInt16();
                                reader.ReadUInt16(); //discard padding
                                break;
                            case 4:
                                DirectoryItems[i].DataValue = reader.ReadUInt32();
                                break;
                        }
                    }
                }

                NextIFDOffset = reader.ReadUInt32(); //Next IFD offset

                ReadDataValues(reader);
            }

            private void ReadDataValues(BinaryReaderFlexiEndian reader)
            {
                foreach (DirectoryItem directoryItem in DirectoryItems)
                {
                    if (directoryItem.DataOffset != 0)
                    {
                        if (directoryItem.TagNumber == ExifTag.ExifOffset)
                        {
                            directoryItem.SubIFDBlock = new IFDBlock(directoryItem.DataOffset, 6);
                            directoryItem.SubIFDBlock.Read(reader);
                        }
                        else
                        {
                            reader.BaseStream.Position = directoryItem.DataOffset + 6;
                            directoryItem.Data = reader.ReadBytes((int)directoryItem.DataLength);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// IFD's directory item entry
        /// tab number | data format | # of component | data value or offset to data value
        ///   2 byte   |   2 byte    |     4 byte     |            4 byte
        /// </summary>
        internal class DirectoryItem
        {
            internal ExifTag TagNumber { get; set; }
            internal DirectoryItemDataFormat DataFormat { get; set; }
            internal uint NumberOfComponents { get; set; }
            internal uint DataValue { get; set; }
            internal uint DataOffset { get; set; }
            internal uint DataLength { get { return NumberOfComponents * GetBytesPerComponent(); } }
            internal byte[] Data { get; set; }
            internal object DecryptedData { get { return GetDecryptedData(); } }
            internal IFDBlock SubIFDBlock { get; set; }
            //internal void ParseData(BinaryReaderFlexiEndian reader)
            //{
            //    switch (DataFormat)
            //    {
            //        case DirectoryItemDataFormat.UnsignedByte:
            //            Data = reader.ReadByte();
            //            break;
            //        case DirectoryItemDataFormat.ASCIIStrings:
            //            Data = Encoding.ASCII.GetString(reader.ReadBytes((int)DataLength - 1));
            //            break;
            //        case DirectoryItemDataFormat.UnsignedShort:
            //            Data = reader.ReadUInt16();
            //            break;
            //        case DirectoryItemDataFormat.UnsignedLong:
            //            Data = reader.ReadUInt64();
            //            break;
            //        case DirectoryItemDataFormat.UnsignedRational:
            //            Data = reader.ReadDouble();
            //            break;
            //        case DirectoryItemDataFormat.SignedByte:
            //            Data = reader.ReadSByte();
            //            break;
            //        case DirectoryItemDataFormat.Undefined:
            //            Data = reader.ReadBytes((int)DataLength);
            //            break;
            //        case DirectoryItemDataFormat.SignedShort:
            //            Data = reader.ReadInt16();
            //            break;
            //        case DirectoryItemDataFormat.SignedLong:
            //            Data = reader.ReadInt64();
            //            break;
            //        case DirectoryItemDataFormat.SignedRational:
            //            Data = reader.ReadDouble();
            //            break;
            //        case DirectoryItemDataFormat.SingleFloat:
            //            Data = reader.ReadSingle();
            //            break;
            //        case DirectoryItemDataFormat.DoubleFloat:
            //            Data = reader.ReadSingle();
            //            break;
            //        default:
            //            Data = null;
            //            break;
            //    }
            //}
            private object GetDecryptedData()
            {
                switch (DataFormat)
                {
                    case DirectoryItemDataFormat.UnsignedByte:
                        return Data[0];
                    case DirectoryItemDataFormat.ASCIIStrings:
                        return Encoding.ASCII.GetString(Data);
                    case DirectoryItemDataFormat.UnsignedShort:
                        return BitConverter.ToUInt16(Data, 0);
                    case DirectoryItemDataFormat.UnsignedLong:
                        return BitConverter.ToUInt64(Data, 0);
                    case DirectoryItemDataFormat.UnsignedRational:
                        return BitConverter.ToDouble(Data, 0); //TODO do proper conversion
                    case DirectoryItemDataFormat.SignedByte:
                        return Convert.ToSByte(Data[0]);
                    case DirectoryItemDataFormat.SignedShort:
                        return BitConverter.ToUInt16(Data, 0);
                    case DirectoryItemDataFormat.SignedLong:
                        return BitConverter.ToInt64(Data, 0);
                    case DirectoryItemDataFormat.SignedRational:
                        return BitConverter.ToDouble(Data, 0); //TODO do proper conversion
                    case DirectoryItemDataFormat.SingleFloat:
                        return BitConverter.ToSingle(Data, 0);
                    case DirectoryItemDataFormat.DoubleFloat:
                        return BitConverter.ToSingle(Data, 0); //TODO do proper conversion
                    case DirectoryItemDataFormat.Undefined:
                    default:
                        return null;
                }
            }

            private uint GetBytesPerComponent()
            {
                switch (DataFormat)
                {
                    case DirectoryItemDataFormat.UnsignedByte:
                    case DirectoryItemDataFormat.ASCIIStrings:
                    case DirectoryItemDataFormat.SignedByte:
                    case DirectoryItemDataFormat.Undefined:
                        return 1;
                    case DirectoryItemDataFormat.UnsignedShort:
                    case DirectoryItemDataFormat.SignedShort:
                        return 2;
                    case DirectoryItemDataFormat.UnsignedLong:
                    case DirectoryItemDataFormat.SignedLong:
                    case DirectoryItemDataFormat.SingleFloat:
                        return 4;
                    case DirectoryItemDataFormat.UnsignedRational:
                    case DirectoryItemDataFormat.SignedRational:
                    case DirectoryItemDataFormat.DoubleFloat:
                        return 8;
                }
                return 0;
            }
        }

        internal enum DirectoryItemDataFormat : ushort
        {
            UnsignedByte = 1,
            ASCIIStrings = 2,
            UnsignedShort = 3,
            UnsignedLong = 4,
            UnsignedRational = 5,
            SignedByte = 6,
            Undefined = 7,
            SignedShort = 8,
            SignedLong = 9,
            SignedRational = 10,
            SingleFloat = 11,
            DoubleFloat = 12
        }
    }
}
