namespace JPEGFileFormatLib.Markers
{
    /// <summary>
    /// Defines comment.
    /// </summary>
    internal class COM : JpegMarkerBase
    {
        public string Comment { get; set; }

        internal COM() : base(JpegMarker.COM)
        {
        }

        public override void ReadExtensionData(BinaryReaderFlexiEndian reader)
        {
            while (reader.PeekChar() != 0x00 && reader.BaseStream.Position != reader.BaseStream.Length)
                Comment += reader.ReadChar();
        }
    }
}
