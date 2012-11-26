using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FileManager2.Extentions
{
    public static class DataReader
    {
        public static byte AsByte(this byte[] buffer, int index)
        {
            return buffer[index];
        }

        public static int AsWord(this byte[] buffer, int index)
        {
            return (buffer[index + 1] << 8) | buffer[index];
        }

        public static int AsInt32(this byte[] buffer, int index)
        {
            int result = buffer[index + 3];
            result <<= 8;
            result |= buffer[index + 2];
            result <<= 8;
            result |= buffer[index + 1];
            result <<= 8;
            result |= buffer[index];

            return result;
        }
    }
}
