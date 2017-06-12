using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPEGFileFormatLib
{
    internal enum JFIFMarkers : UInt16
    {
        SOF0 = 0xFFC0,
        SOF1 = 0xFFC1,
        SOF2 = 0xFFC2,
        SOF3 = 0xFFC3,
        DHT = 0xFFC4,
        SOF5 = 0xFFC5,
        SOF6 = 0xFFC6,
        SOF7 = 0xFFC7,
        JPG = 0xFFC8,
        SOF9 = 0xFFC9,
        SOF10 = 0xFFCA,
        SOF11 = 0xFFCB,
        DAC = 0xFFCC,
        SOF13 = 0xFFCD,
        SOF14 = 0xFFCE,
        SOF15 = 0xFFCF,
        RST0 = 0xFFD0,
        RST1 = 0xFFD1,
        RST2 = 0xFFD2,
        RST3 = 0xFFD3,
        RST4 = 0xFFD4,
        RST5 = 0xFFD5,
        RST6 = 0xFFD6,
        RST7 = 0xFFD7,
        SOI = 0xFFD8,
        EOI = 0xFFD9,
        SOS = 0xFFDA,
        DQT = 0xFFDB,
        DNL = 0xFFDC,
        DRI = 0xFFDD,
        DHP = 0xFFDE,
        EXP = 0xFFDF,
        APP0 = 0xFFE0,
        APP1 = 0xFFE1,
        APP2 = 0xFFE2,
        APP3 = 0xFFE3,
        APP4 = 0xFFE4,
        APP5 = 0xFFE5,
        APP6 = 0xFFE6,
        APP7 = 0xFFE7,
        APP8 = 0xFFE8,
        APP9 = 0xFFE9,
        APP10 = 0xFFEA,
        APP11 = 0xFFEB,
        APP12 = 0xFFEC,
        APP13 = 0xFFED,
        APP14 = 0xFFEE,
        APP15 = 0xFFEF,
        JPG6 = 0xFFF0,
        JPG7 = 0xFFF7,
        JPG8 = 0xFFF8,
        JPG9 = 0xFFF9,
        JPG10 = 0xFFFA,
        JPG11 = 0xFFFB,
        JPG12 = 0xFFFC,
        JPG13 = 0xFFFD,
        COM = 0xFFFE
    }
}
