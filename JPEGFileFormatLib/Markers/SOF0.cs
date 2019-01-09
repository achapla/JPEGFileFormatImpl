using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPEGFileFormatLib.Markers
{
    /// <summary>
    /// FF C0
    /// </summary>
    internal class SOF0 : JpegMarkerBase
    {
        public byte Precision { get; set; } //bits/sample
        public ushort ImageY { get; set; }
        public ushort ImageX { get; set; }
        public byte NumerOfComponents { get; set; } //1 = Grey Scaled, 3 = Color YCbCr or YIQ, 4 = color CMYK
        public SOF0Component[] Components { get; set; }

        public SOF0() : base(JpegMarker.SOF0)
        {
        }

        public override void ReadExtensionData(BinaryReaderFlexiEndian reader)
        {
            Precision = reader.ReadByte();
            ImageY = reader.ReadUInt16();
            ImageX = reader.ReadUInt16();
            NumerOfComponents = reader.ReadByte();
            Components = new SOF0Component[NumerOfComponents];

            for (int i = 0; i < Components.Length; i++)
                Components[i] = new SOF0Component(reader);
        }

        internal class SOF0Component
        {
            readonly byte componentId;
            readonly byte samplingFactors; //Samp Fac=0x11 (Subsamp 1 x 1) (4:4:4)
            readonly byte quantizationTableNumber; //0x00 (Lum: Y), 0x01 (Chrom: Cb), 0x01 (Chrom: Cr)
            int vertical { get { return samplingFactors & 0x0F; } }
            int horizontal { get { return samplingFactors >> 4; } }

            internal SOF0Component(BinaryReaderFlexiEndian reader)
            {
                componentId = reader.ReadByte();
                samplingFactors = reader.ReadByte();
                quantizationTableNumber = reader.ReadByte();
            }
        }
    }
}