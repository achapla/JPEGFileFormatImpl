using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPEGFileFormatLib
{
    /// <summary>
    /// https://www.w3.org/Graphics/JPEG/itu-t81.pdf
    /// </summary>
    public class JPEGFile : IDisposable
    {
        public string FilePath;

        BinaryReaderFlexiEndian _reader;
        JPEGHeader _header;

        public JPEGFile(string filePath)
        {
            FilePath = filePath;

            _header = new JPEGHeader();
            _reader = new BinaryReaderFlexiEndian(new MemoryStream(File.ReadAllBytes(filePath)));
            _reader.UseBigEndian = true;

            _header.ReadHeader(_reader);
        }

        public void Dispose()
        {
            if (_reader != null)
                _reader.Dispose();
        }
    }
}
