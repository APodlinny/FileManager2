using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FileManager2.Extentions;

namespace FileManager2.Model
{
    class UnknownPartition : IPartition
    {
        private PartitionInfo _info;

        public UnknownPartition(PartitionInfo info)
        {
            _info = info;
        }

        public PartitionInfo Info()
        {
            return _info;
        }

        public IEnumerable<DirectoryObject> CurrentDirectoryContents()
        {
            throw new ErrorException("Can't read directory contents from unknown partition.");
        }

        public string CurrentPath()
        {
            return "\\";
        }

        public void GoSubDirectory(string name)
        {
            throw new ErrorException("Can't read directory contents from unknown partition.");
        }

        public void GoRootDirectory()
        {
        }

        public byte[] ReadFile(string name)
        {
            throw new ErrorException("Can't read file contents from unknown partition.");
        }
    }
}
