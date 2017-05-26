using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPEGFileFormatLib
{
    internal class BinaryReaderFlexiEndian : BinaryReader
    {
        internal bool UseBigEndian;
        public BinaryReaderFlexiEndian(System.IO.Stream stream) : base(stream) { }
        public override Int16 ReadInt16()
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
        public override Int32 ReadInt32()
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
        public override Int64 ReadInt64()
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
        public override UInt16 ReadUInt16()
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
        public override UInt32 ReadUInt32()
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
        public override UInt64 ReadUInt64()
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
