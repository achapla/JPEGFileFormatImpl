using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPEGFileFormatLib
{
    public class JPEGFile : IDisposable
    {
        public string FilePath;

        BinaryReaderFlexiEndian _reader;
        JPEGHeader _header;

        public JPEGFile(string filePath)
        {
            this.FilePath = filePath;

            _header = new JPEGHeader();
            _reader = new BinaryReaderFlexiEndian(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));

            _header.ReadHeader(_reader);
        }

        public void Dispose()
        {
            if (_reader != null)
                _reader.Dispose();
        }
    }
}
