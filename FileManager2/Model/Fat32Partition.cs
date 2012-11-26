using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FileManager2.Extentions;

namespace FileManager2.Model
{
    class Fat32Partition : FatPartition, IPartition
    {
        private PartitionInfo _info;

        public Fat32Partition(PartitionInfo info)
        {
            _info = base.info = info;

            diskFileName = String.Format("\\\\.\\{0}:", info.Letter);
            rootDirectoryName = String.Format("{0}:\\", info.Letter);

            bytesPerSector = GetBytesPerSector();
            diskFile = new UnmanagedFile(diskFileName);
            byte[] fatInfo = diskFile.ReadFile(bytesPerSector);

            fatCopiesNumber = fatInfo.AsWord(16);
            fatSize = fatInfo.AsWord(36);
            reservedAreaSize = fatInfo.AsWord(14);
            sectorsPerCluster = fatInfo.AsByte(13);
            fatAddress = reservedAreaSize;
            rootDirectoryCluster = fatInfo.AsInt32(44);

            bitsPerSector = GetBitsNumber(bytesPerSector);
            bytesPerCluster = bytesPerSector * sectorsPerCluster;
            fatContentSize = (uint)(fatSize * bytesPerSector);

            fatContent = LoadFatContent();
            info.FreeSpace = GetFreeDiskSpace();

            currentDirectory = new FatDirectoryObject
            {
                Name = String.Empty,
                Type = DirectoryObjectType.Directory,
                Cluster = rootDirectoryCluster
            };

            directoryStack.Push(currentDirectory);
            LoadCurrentDirectoryObjects();
        }

        public PartitionInfo Info()
        {
            return _info;
        }

        public IEnumerable<DirectoryObject> CurrentDirectoryContents()
        {
            return contents;
        }

        public string CurrentPath()
        {
            return PathFromStack();
        }

        public new void GoSubDirectory(string name)
        {
            base.GoSubDirectory(name);
            LoadCurrentDirectoryObjects();
        }

        public new void GoRootDirectory()
        {
            base.GoRootDirectory();
            LoadCurrentDirectoryObjects();
        }

        public new byte[] ReadFile(string name)
        {
            return base.ReadFile(name);
        }

        private void LoadCurrentDirectoryObjects()
        {
            var objectBuffers =
                GetObjectClusters(currentDirectory.Cluster).
                SelectMany(GetClusterObjectBuffers);

            contents = DirectoryObjectBufferProcessor.
                Process(objectBuffers).
                Where(x => (x.Name != "..") && (x.Name != ".")).
                ToList();
        }

        private int GetFreeDiskSpace()
        {
            diskFile.MovePointer(0, 0);
            uint len = 0x400;
            var rawData = diskFile.ReadFile(len);

            rawData = Enumerable.Range(0x200, 0x200).Select(x => rawData[x]).ToArray();
            int freeClusters = rawData.AsInt32(0x1E8);
            int freeSpace = (int)(freeClusters * bytesPerCluster / 1024.0 / 1024.0);

            return freeSpace;
        }
    }
}
