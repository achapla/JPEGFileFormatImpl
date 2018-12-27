using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPEGFileFormatLib.Markers
{
    internal interface IJpegMarker
    {
        UInt16 Marker { get; set; }
        UInt16 MarkerSize { get; set; }
    }
}
