using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPEGFileFormatLib
{
    /// <summary>
    /// FF DD - Define Restart Interval
    /// </summary>
    internal class DRI
    {
        long start;
        UInt16 length;
        UInt16 RestartInterval;

        internal DRI(BinaryReaderFlexiEndian reader)
        {
            start = reader.BaseStream.Position;
            length = reader.ReadUInt16();
            RestartInterval = reader.ReadUInt16();
        }
    }
}
