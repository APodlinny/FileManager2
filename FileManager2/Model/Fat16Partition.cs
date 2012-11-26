using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FileManager2.Extentions;

namespace FileManager2.Model
{
    class Fat16Partition : FatPartition, IPartition
    {
        private PartitionInfo _info;

        public Fat16Partition(PartitionInfo info)
        {
            _info = base.info = info;

            diskFileName = String.Format("\\\\.\\{0}:", info.Letter);
            rootDirectoryName = String.Format("{0}:\\", info.Letter);

            bytesPerSector = GetBytesPerSector();
            diskFile = new UnmanagedFile(diskFileName);
            byte[] fatInfo = diskFile.ReadFile((uint)bytesPerSector);

            fatCopiesNumber = fatInfo.AsWord(16);
            fatSize = fatInfo.AsWord(22);
            reservedAreaSize = fatInfo.AsWord(14);
            sectorsPerCluster = fatInfo.AsByte(13);
            fatAddress = reservedAreaSize;
            rootDirectoryEntities = fatInfo.AsWord(17);
            rootDirectorySector = reservedAreaSize + (fatCopiesNumber * fatSize);
            bitsPerSector = GetBitsNumber(bytesPerSector);
            bytesPerCluster = bytesPerSector * sectorsPerCluster;
            fatContentSize = (uint)(fatSize * bytesPerSector);

            fatContent = LoadFatContent();
            info.FreeSpace = GetFreeDiskSpace();

            currentDirectory = new FatDirectoryObject
            {
                Name = String.Empty,
                Type = DirectoryObjectType.Directory,
                Cluster = 0
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
            if (currentDirectory.Cluster != 0)
            {
                var objectBuffers =
                    GetObjectClusters(currentDirectory.Cluster).
                    SelectMany(GetClusterObjectBuffers);

                contents = DirectoryObjectBufferProcessor.
                    Process(objectBuffers).
                    Where(x => (x.Name != "..") && (x.Name != ".")).
                    ToList();
            }
            else
            {
                var objectBuffers = GetSectorObjectBuffers(rootDirectorySector);
                contents = DirectoryObjectBufferProcessor.
                    Process(objectBuffers).
                    Where(x => (x.Name != "..") && (x.Name != ".")).
                    ToList();
            }
        }

        private int GetFreeDiskSpace()
        {
            var sectors = Enumerable.Range(0, fatSize).
                Select(x => Enumerable.Range(x * (int)bytesPerSector, (int)bytesPerSector).
                    Select(y => fatContent[y]).
                    ToArray()).
                ToList();

            var freeClusters = sectors.
                Select(s => Enumerable.Range(0, (int)bytesPerSector / 2).
                    Select(i => s.AsWord(i * 2)).
                    Count(w => w == 0)).
                Sum();

            return (int)(freeClusters * bytesPerCluster / 1024.0 / 1024.0);
        }
    }
}
