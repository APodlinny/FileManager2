using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FileManager2.Model
{
    public class FatDirectoryObject : DirectoryObject
    {
        public int Cluster { get; set; }
    }
}
