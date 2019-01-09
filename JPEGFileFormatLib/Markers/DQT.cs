using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPEGFileFormatLib.Markers
{
    internal class DQT : JpegMarkerBase
    {
        public DQTStruct[] QuantizationTables { get; set; }

        internal DQT() : base(JpegMarker.DQT)
        {
        }

        public override void ReadExtensionData(BinaryReaderFlexiEndian reader)
        {
            QuantizationTables = new DQTStruct[(MarkerSize - 2) / 65];
            for (int i = 0; i < QuantizationTables.Length; i++)
                QuantizationTables[i] = new DQTStruct(reader);
        }

        /// <summary>
        /// http://vip.sugovica.hu/Sardi/kepnezo/JPEG%20File%20Layout%20and%20Format.htm
        /// </summary>
        internal class DQTStruct
        {
            /// <summary>
            /// bit 0..3: number of QT (0..3, otherwise error)
            /// bit 4..7: precision of QT, 0 = 8 bit, otherwise 16 bit
            /// </summary>
            public byte QuantizationTableInformation { get; set; } // 0 : Luminance, 1 : Chrominance
            /// <summary>
            /// Array contain quantization table data values.
            /// </summary>
            public byte[] Data { get; set; }
            /// <summary>
            /// Defines index of current quantization table. TODO: Fix this later.
            /// </summary>
            public byte NumberOfQuantizationTable { get { return (byte)(QuantizationTableInformation & 0x0F); } }
            /// <summary>
            /// Used to identify how many bit of length is for each value. 1 = 16 bits, 0 = 8 bits TODO: Fix this later.
            /// </summary>
            public byte Precision { get { return (byte)(QuantizationTableInformation & 0xF0); } }

            internal DQTStruct(BinaryReaderFlexiEndian reader)
            {
                QuantizationTableInformation = reader.ReadByte();
                Data = reader.ReadBytes(64 * (Precision + 1));
            }
        }
    }
}
