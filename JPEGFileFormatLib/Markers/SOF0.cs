using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPEGFileFormatLib
{
    /// <summary>
    /// FF C0
    /// </summary>
    internal class SOF0
    {
        UInt16 length;
        byte precision; //bits/sample
        UInt16 imageY;
        UInt16 imageX;
        byte numerOfComponents; //1 = Grey Scaled, 3 = Color YCbCr or YIQ, 4 = color CMYK
        SOF0Component[] components;

        internal SOF0(BinaryReaderFlexiEndian reader)
        {
            length = reader.ReadUInt16(); //(UInt16)(reader.ReadByte() * 256 + reader.ReadByte()); //Length of structure
            precision = reader.ReadByte();
            imageY = reader.ReadUInt16();
            imageX = reader.ReadUInt16();
            numerOfComponents = reader.ReadByte();
            components = new SOF0Component[numerOfComponents];

            for (int i = 0; i < components.Length; i++)
                components[i] = new SOF0Component(reader);
        }

        internal class SOF0Component
        {
            byte componentId;
            byte samplingFactors; //Samp Fac=0x11 (Subsamp 1 x 1) (4:4:4)
            byte quantizationTableNumber; //0x00 (Lum: Y), 0x01 (Chrom: Cb), 0x01 (Chrom: Cr)
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