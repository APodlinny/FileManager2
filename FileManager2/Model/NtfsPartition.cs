using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace FileManager2.Model
{
    class NtfsPartition : IPartition
    {
        private PartitionInfo _info;
        private DriveInfo _drive;
        private DirectoryInfo _currentDirectory;

        public NtfsPartition(PartitionInfo info)
        {
            _info = info;
            _drive = new DriveInfo(info.Letter.ToString());
            info.FreeSpace = (int)(_drive.AvailableFreeSpace / 1024.0 / 1024.0);
            _currentDirectory = _drive.RootDirectory;
        }

        public PartitionInfo Info()
        {
            return _info;
        }

        public IEnumerable<DirectoryObject> CurrentDirectoryContents()
        {
            var files = _currentDirectory.GetFiles().Select(x => new DirectoryObject
            {
                Name = x.Name,
                Type = DirectoryObjectType.File
            });

            var directories = _currentDirectory.GetDirectories().Select(x => new DirectoryObject
            {
                Name = x.Name,
                Type = DirectoryObjectType.Directory
            });

            return directories.Concat(files);
        }

        public string CurrentPath()
        {
            return _currentDirectory.FullName.Remove(0, 2);
        }

        public void GoSubDirectory(string name)
        {
            var subDirectories = _currentDirectory.EnumerateDirectories().Where(x => x.Name == name);
            if (subDirectories.Any())
            {
                _currentDirectory = subDirectories.First();
            }
        }

        public void GoRootDirectory()
        {
            _currentDirectory = _currentDirectory.Parent;
        }

        public byte[] ReadFile(string name)
        {
            var files = _currentDirectory.EnumerateFiles().Where(x => x.Name == name);
            if (files.Any())
            {
                return File.ReadAllBytes(files.First().FullName);
            }

            return new byte[] { };
        }
    }
}
