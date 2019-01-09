namespace JPEGFileFormatLib.Markers
{
    /// <summary>
    /// FF EE
    /// </summary>
    internal class APP14 : JpegMarkerBase
    {
        public string Tag { get; set; }
        public byte DCTEncodeVersion { get; set; }
        public ushort Flag0 { get; set; } //0x0 = (none), Bit 15 = Encoded with Blend=1 downsampling
        public ushort Flag1 { get; set; }
        public byte ColorTransform { get; set; } //0 = Unknown (RGB or CMYK), 1 = YCbCr, 2 = YCCK

        internal APP14() : base(JpegMarker.APP14)
        {
        }

        public override void ReadExtensionData(BinaryReaderFlexiEndian reader)
        {
            while (reader.PeekChar() != 0)
                Tag += reader.ReadChar();

            reader.ReadByte(); //Discard null terminator

            DCTEncodeVersion = reader.ReadByte();
            Flag0 = reader.ReadUInt16();
            Flag1 = reader.ReadUInt16();
            ColorTransform = reader.ReadByte(); // 1: YCbCr
        }
    }
}
