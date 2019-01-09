using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPEGFileFormatLib.Markers
{
    /// <summary>
    /// FF DD - Define Restart Interval
    /// </summary>
    internal class DRI : JpegMarkerBase
    {
        /// <summary>
        /// This is in units of MCU blocks, means that every n MCU blocks a RSTn marker can be found. The first marker will be RST0, then RST1 etc, after RST7 repeating from RST0. 0 value means no restart interval.
        /// </summary>
        public ushort RestartInterval { get; set; }

        internal DRI() : base(JpegMarker.DRI)
        {
        }

        public override void ReadExtensionData(BinaryReaderFlexiEndian reader)
        {
            RestartInterval = reader.ReadUInt16();
        }

        public void ProcessCurrent(BinaryReaderFlexiEndian reader)
        {
            reader.FillBitData();
            reader.DiscardFirstOrphanByte();

            bool isRestartMarker = (reader.TakeBits(16) & 0xFFD0) == 0xFFD0;
            if (!isRestartMarker)
                throw new Exception();
            reader.RemoveBits(16);
        }
    }
}
