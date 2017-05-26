using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPEGFileFormatLib
{
    class Program
    {
        static void Main(string[] args)
        {
            JPEGFile j = new JPEGFile(@"C:\Users\achapla\Desktop\Images\kiss-smiley-pillow.jpg");
            //JPEGFile j = new JPEGFile(@"C:\Ent\Wall\iron_man_comic_art-wallpaper-1920x1080.jpg");
            //JPEGFile j = new JPEGFile(@"C:\Ent\Wall\car.jpg");
            //JPEGFile j = new JPEGFile(@"C:\Ent\Wall\Spotlight\8ac03762be82b645a72d89d2d1b3bf06d1b48c1580d8ae76ec07ac3945df65c7.jpg");
        }
    }
}
