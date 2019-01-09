using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPEGFileFormatLib.Markers
{
    /// <summary>
    /// FF DA
    /// </summary>
    internal class SOS : JpegMarkerBase
    {
        public byte NumberOfComponentsInScan { get; set; }
        public SOSComponent[] Components { get; set; }
        public byte StartOfSpectralSelection { get; set; }
        public byte EndOfSpectralSelection { get; set; }
        public byte ApproximationData { get; set; }
        public byte ApproximationHigh { get { return (byte)(ApproximationData & 0x0F); } }
        public byte ApproximationLow { get { return (byte)(ApproximationData >> 4); } }
        public byte[] CompressedData { get; set; }

        public SOS() : base(JpegMarker.SOS)
        {
        }

        public override void ReadExtensionData(BinaryReaderFlexiEndian reader)
        {
            Components = new SOSComponent[NumberOfComponentsInScan = reader.ReadByte()];

            for (int i = 0; i < NumberOfComponentsInScan; i++)
                Components[i] = new SOSComponent(reader);

            StartOfSpectralSelection = reader.ReadByte();
            EndOfSpectralSelection = reader.ReadByte();
            ApproximationData = reader.ReadByte();
        }

        internal class SOSComponent
        {
            readonly byte componentId;
            readonly byte huffmanTableToUse; //table=0(DC),0(AC) ==> table=1(DC),1(AC) ==> table=1(DC),1(AC)

            internal SOSComponent(BinaryReaderFlexiEndian reader)
            {
                componentId = reader.ReadByte();
                huffmanTableToUse = reader.ReadByte();
            }
        }
    }
}
