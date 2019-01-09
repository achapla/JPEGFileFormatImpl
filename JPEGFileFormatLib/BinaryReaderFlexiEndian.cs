using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace JPEGFileFormatLib
{
    /// <summary>
    /// More robust alternative Be.IO
    /// </summary>
    internal class BinaryReaderFlexiEndian : BinaryReader
    {
        internal bool UseBigEndian { get; set; }
        internal ulong DataBuffer { get; set; }
        private int removedBits = 64;

        public BinaryReaderFlexiEndian(System.IO.Stream stream) : base(stream) { }

        public void FillBitData()
        {
            if (BaseStream.Length - BaseStream.Position < removedBits / 8)
                removedBits = removedBits % 8;

            switch (removedBits / 8)
            {
                case 8:
                    DataBuffer = ReadUInt64();
                    break;
                case 4:
                    DataBuffer = DataBuffer | ReadUInt32() << (removedBits % 8);
                    break;
                case 3:
                    DataBuffer = DataBuffer | (uint)(ReadUInt16() << ((removedBits % 8) + 8)) | (uint)(ReadByte() << (removedBits % 8));
                    break;
                case 2:
                    DataBuffer = DataBuffer | (uint)ReadUInt16() << (removedBits % 8);
                    break;
                case 1:
                    DataBuffer = DataBuffer | (uint)(ReadByte() << (removedBits % 8));
                    break;
                default:
                    break;
            }
            removedBits = removedBits % 8;

            RemoveStuffByte();
        }

        private void RemoveStuffByte()
        {
            //int shiftedByte = removedBits;
            //while (shiftedByte <= 56)
            //{
            //    if ((DataBuffer >> shiftedByte & 0x0000FFFF) == 0x0000FF00)
            //    {
            //        ulong temp = DataBuffer >> (shiftedByte + 8);
            //        ulong temp1 = DataBuffer << (64 - shiftedByte) >> (64 - shiftedByte);
            //        if (shiftedByte == 0) //To handle special case when data shifted by size of data nothing happends, it should be 0
            //            temp1 = 0;
            //        ulong temp2 = temp << shiftedByte;
            //        ulong temp3 = temp1 | temp2;
            //        ulong temp4 = temp3 << 8;
            //        DataBuffer = temp4;
            //        removedBits = removedBits + 8;
            //        FillBitData();
            //        break;
            //    }
            //    shiftedByte += 8;
            //}

            //if (DataBuffer != DataBuffer)
            //    return;
            ulong originalDataBufferValue = DataBuffer;
            originalDataBufferValue = originalDataBufferValue >> removedBits;
            ulong cnst = 0xFF00u;
            ulong mask = 0xFFFFu;
            int moved = 0;
            while (moved <= 48)
            {
                if ((originalDataBufferValue & mask) == cnst)
                {
                    originalDataBufferValue = (originalDataBufferValue & (ulong.MaxValue << (moved + 8))) | ((originalDataBufferValue & ~(ulong.MaxValue << moved)) << 8);
                    DataBuffer = originalDataBufferValue << removedBits;
                    removedBits = removedBits + 8;
                    FillBitData();
                    break;
                }

                cnst = cnst << 8;
                mask = mask << 8;
                moved += 8;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort TakeBits(int length)
        {
            return (ushort)(DataBuffer >> (64 - length));
        }

        public ulong RemoveBits(int length)
        {
            removedBits += length;
            DataBuffer = DataBuffer << length;
            return DataBuffer;
        }

        public void DiscardFirstOrphanByte()
        {
            if (removedBits == 0)
                return;

            DataBuffer = DataBuffer << (8 - removedBits);
            removedBits = 8;
        }

        public override short ReadInt16()
        {
            if (UseBigEndian)
            {
                byte[] a16 = base.ReadBytes(2);
                Array.Reverse(a16);
                return BitConverter.ToInt16(a16, 0);
            }
            else
                return base.ReadInt16();
        }

        public override int ReadInt32()
        {
            if (UseBigEndian)
            {
                byte[] a32 = base.ReadBytes(4);
                Array.Reverse(a32);
                return BitConverter.ToInt32(a32, 0);
            }
            else
                return base.ReadInt32();
        }

        public override long ReadInt64()
        {
            if (UseBigEndian)
            {
                byte[] a64 = base.ReadBytes(8);
                Array.Reverse(a64);
                return BitConverter.ToInt64(a64, 0);
            }
            else
                return base.ReadInt64();
        }

        public override ushort ReadUInt16()
        {
            if (UseBigEndian)
            {
                byte[] a16 = base.ReadBytes(2);
                Array.Reverse(a16);
                return BitConverter.ToUInt16(a16, 0);
            }
            else
                return base.ReadUInt16();
        }

        public override uint ReadUInt32()
        {
            if (UseBigEndian)
            {
                byte[] a32 = base.ReadBytes(4);
                Array.Reverse(a32);
                return BitConverter.ToUInt32(a32, 0);
            }
            else
                return base.ReadUInt32();
        }

        public override ulong ReadUInt64()
        {
            if (UseBigEndian)
            {
                byte[] a64 = base.ReadBytes(8);
                Array.Reverse(a64);
                return BitConverter.ToUInt64(a64, 0);
            }
            else
                return base.ReadUInt64();
        }
    }
}
