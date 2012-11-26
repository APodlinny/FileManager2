using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FileManager2.Model
{
    public interface IPartition
    {
        PartitionInfo Info();
        IEnumerable<DirectoryObject> CurrentDirectoryContents();
        string CurrentPath();
        void GoSubDirectory(string name);
        void GoRootDirectory();
        byte[] ReadFile(string name);
    }
}
