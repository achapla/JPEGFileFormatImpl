using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPEGFileFormatLib
{
    internal class DirectoryItem
    {
        internal ExifTag tagNumber;
        internal UInt16 dataFormat;
        internal UInt32 numberOfComponents;
        internal UInt32 dataValue;
        internal UInt32 dataOffset;
        internal UInt32 DataLength { get { return numberOfComponents * GetBytesPerComponent(); } }

        private UInt32 GetBytesPerComponent()
        {
            switch (dataFormat)
            {
                case 1:
                case 2:
                case 6:
                case 7:
                    return 1;
                case 3:
                case 8:
                    return 2;
                case 4:
                case 9:
                case 11:
                    return 4;
                case 5:
                case 10:
                case 12:
                    return 8;
            }
            return 0;
        }
    }
}
