using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
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
        internal ulong DataBuffer2 { get { return DataBuffer >> removedBits; } }
        private int removedBits = 64;
        private int bytesToScan = 0;

        public BinaryReaderFlexiEndian(System.IO.Stream stream) : base(stream) { }

        public void FillBitData()
        {
            if (removedBits < 8)
                return;

            if (BaseStream.Length - BaseStream.Position < removedBits / 8)
                removedBits = removedBits % 8;

            bytesToScan += removedBits / 8;
            switch (removedBits / 8)
            {
                case 8:
                    DataBuffer = ReadUInt64();
                    break;
                case 4:
                    DataBuffer = DataBuffer | ((ulong)ReadUInt32() << (removedBits % 8));
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

            if (bytesToScan >= 2)
                RemoveStuffByte();
        }

        private void RemoveStuffByte()
        {
            ulong originalDataBufferValue = DataBuffer >> removedBits;
            ulong cnst = 0xFF00ul << ((bytesToScan - 2) * 8);
            ulong mask = 0xFFFFul << ((bytesToScan - 2) * 8);

            while ((mask & 0xFFFF) != 0xFFFF)
            {
                if ((originalDataBufferValue & mask) == cnst)
                {
                    int byteIndexToRemoveFromRight = bytesToScan - 2;
                    ulong leftPart = originalDataBufferValue & (ulong.MaxValue << (byteIndexToRemoveFromRight * 8));
                    ulong rightPart = originalDataBufferValue & ~(ulong.MaxValue << (byteIndexToRemoveFromRight * 8));
                    rightPart = rightPart << 8;
                    originalDataBufferValue = leftPart | rightPart;
                    DataBuffer = originalDataBufferValue << removedBits;
                    removedBits = removedBits + 8;
                    bytesToScan -= 2;
                    FillBitData();
                    break;
                }
                mask = mask >> 8;
                cnst = cnst >> 8;
                --bytesToScan;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort TakeBits(int length)
        {
            return (ushort)(DataBuffer >> (64 - length));
        }

        public bool IsEOF()
        {
            return (((DataBuffer2 >> 48) & 0xFFFF) == 0xFFD9) || (((DataBuffer2 >> 40) & 0xFFFF) == 0xFFD9) || (((DataBuffer2 >> 32) & 0xFFFF) == 0xFFD9);
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
