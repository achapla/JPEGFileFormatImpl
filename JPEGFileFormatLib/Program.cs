using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPEGFileFormatLib
{
    class Program
    {
        static void Main(string[] args)
        {
            //JPEGFile j = new JPEGFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Samples", "kiss-smiley-pillow.jpg"));
            JPEGFile j = new JPEGFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Samples", "fig2.jpg"));
            //JPEGFile j = new JPEGFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Samples", "test.jpg"));
            //JPEGFile j = new JPEGFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Samples", "iron_man.jpg"));
            //JPEGFile j = new JPEGFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Samples", "car.jpg"));
        }
    }
}
