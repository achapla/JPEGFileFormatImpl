using System.IO;

namespace JPEGFileFormatLib.Markers
{
    /// <summary>
    /// App markers : https://sno.phy.queensu.ca/~phil/exiftool/TagNames/JPEG.html
    /// </summary>
    internal abstract class JpegMarkerBase : IJpegMarker
    {
        public long StartPosition { get; private set; }
        public JpegMarker Marker { get; private set; }
        public ushort MarkerSize { get; private set; }

        public JpegMarkerBase(JpegMarker marker)
        {
            Marker = marker;
        }

        public virtual void Read(BinaryReaderFlexiEndian reader)
        {
            ReadMarkerSize(reader);
            using (MemoryStream app1DataStream = new MemoryStream(reader.ReadBytes(MarkerSize - 2)))
            using (BinaryReaderFlexiEndian markerDataReader = new BinaryReaderFlexiEndian(app1DataStream) { UseBigEndian = true })
                ReadExtensionData(markerDataReader);
        }

        private void ReadMarkerSize(BinaryReaderFlexiEndian reader)
        {
            StartPosition = reader.BaseStream.Position;
            MarkerSize = reader.ReadUInt16(); // size of marker data (incl. marker)
        }

        public abstract void ReadExtensionData(BinaryReaderFlexiEndian reader);
    }
}
