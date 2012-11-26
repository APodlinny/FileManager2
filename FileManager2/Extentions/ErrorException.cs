using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FileManager2.Extentions
{
    class ErrorException : Exception
    {
        public ErrorException(string message) : base(message)
        {
        }

        public ErrorException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
