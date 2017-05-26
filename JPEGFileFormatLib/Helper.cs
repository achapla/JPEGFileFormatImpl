using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPEGFileFormatLib
{
    public static class Helpers
    {
        // Note this MODIFIES THE GIVEN ARRAY then returns a reference to the modified array.
        public static byte[] Reverse(this byte[] b, int count = 0, int start = 0)
        {
            return b.Skip(start).Take(count).Reverse().ToArray();
        }
    }
}
