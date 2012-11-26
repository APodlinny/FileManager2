using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using FileManager2.Extentions;

namespace FileManager2.Model
{
    public abstract class FatPartition
    {
        protected PartitionInfo info;

        protected string diskFileName;
        protected string rootDirectoryName;
        protected int rootDirectoryCluster;
        protected uint bytesPerSector;
        protected int bitsPerSector;
        protected uint bytesPerCluster;
        protected byte sectorsPerCluster;

        protected int fatCopiesNumber;
        protected int fatSize;
        protected int reservedAreaSize;
        protected byte[] fatContent;
        protected int fatAddress;
        protected uint fatContentSize;
        protected int rootDirectorySector;
        protected int rootDirectoryEntities;

        protected FatDirectoryObject currentDirectory;
        protected UnmanagedFile diskFile;

        protected Stack<DirectoryObject> directoryStack = new Stack<DirectoryObject>();
        protected List<FatDirectoryObject> contents;

        protected uint GetBytesPerSector()
        {
            uint sectorsPerCluster;
            uint lpBytesPerSector;
            uint numberOfFreeClusters;
            uint totalNumberOfClusters;

            GetDiskFreeSpace(rootDirectoryName, out sectorsPerCluster, out lpBytesPerSector,
               out numberOfFreeClusters, out totalNumberOfClusters);

            return lpBytesPerSector;
        }

        protected static int GetBitsNumber(uint x)
        {
            uint i = x;
            int n = 0;

            while ((i = i >> 1) != 0)
            {
                n++;
            }

            return n;
        }

        protected byte[] LoadFatContent()
        {
            diskFile.MovePointer(fatAddress << bitsPerSector);

            return diskFile.ReadFile(fatContentSize);
        }

        protected string PathFromStack()
        {
            if ((directoryStack.Count >= 1) && !String.IsNullOrEmpty(directoryStack.ElementAt(0).Name))
            {
                var parts = directoryStack.Select(x => x.Name).Reverse();
                return String.Join("\\", parts) + "\\";
            }
            else
            {
                return "\\";
            }
        }

        protected byte[] ReadFile(string name)
        {
            var files = contents.Where(x => (x.Type == DirectoryObjectType.File) && (x.Name == name));

            if (files.Any())
            {
                var rawData =
                    GetObjectClusters(files.First().Cluster).
                    SelectMany(GetClusterObjectBuffers).
                    SelectMany(x => x);

                var fileLen = files.First().Size;

                if (fileLen < rawData.Count())
                {
                    return rawData.Take(fileLen).ToArray();
                }

                return rawData.ToArray();
            }

            return new byte[] { };
        }

        protected IEnumerable<byte[]> GetClusterObjectBuffers(int cluster)
        {
            return GetSectorObjectBuffers(GetSectorByCluster(cluster));
        }

        protected IEnumerable<byte[]> GetSectorObjectBuffers(int sector)
        {
            diskFile.MovePointer(sector << bitsPerSector,
                sector >> (32 - bitsPerSector));

            var buffer = diskFile.ReadFile(bytesPerCluster);

            for (int i = 0; i < buffer.Length / 32; i++)
            {
                if ((buffer[i * 32] != 0xE5) &&
                    (buffer[i * 32] != 0x00))
                {
                    var directoryObject = new byte[32];

                    for (int j = 0; j < 32; j++)
                    {
                        directoryObject[j] = buffer[j + (i * 32)];
                    }

                    yield return directoryObject;
                }

                if (buffer[i * 32] == 0x00)
                {
                    yield break;
                }
            }
        }

        protected IEnumerable<int> GetObjectClusters(int cluster)
        {
            yield return cluster;

            while (true)
            {
                cluster = GetNextCluster(cluster);
                if (IsLastCluster(cluster))
                {
                    yield break;
                }
                else
                {
                    yield return cluster;
                }
            }
        }

        protected int GetNextCluster(int cluster)
        {
            return fatContent.AsInt32(cluster * (info.FileSystem == FileSystemType.Fat32 ? 4 : 2));
        }

        protected bool IsLastCluster(int cluster)
        {
            return (uint)cluster >= (info.FileSystem == FileSystemType.Fat32 ? 0x0FFFFFF8 : 0xFFF8);
        }

        protected int GetSectorByCluster(int cluster)
        {
            if (info.FileSystem == FileSystemType.Fat32)
            {
                return reservedAreaSize + (fatCopiesNumber * fatSize) + ((cluster - 2) * sectorsPerCluster);
            }
            else
            {
                if (cluster != 0)
                {
                    return (int)(rootDirectorySector + ((cluster - 2) * sectorsPerCluster) +
                                (rootDirectoryEntities * 32 / bytesPerSector));
                }

                return rootDirectorySector;
            }
        }

        protected void GoSubDirectory(string name)
        {
            var subDirectories = contents.Where(x => (x.Type == DirectoryObjectType.Directory) &&
                           (x.Name == name));

            if (subDirectories.Any())
            {
                var subDirectory = subDirectories.First();
                currentDirectory = subDirectory;
                directoryStack.Push(currentDirectory);
            }
        }

        protected void GoRootDirectory()
        {
            if (directoryStack.Count > 1)
            {
                directoryStack.Pop();
                currentDirectory = (FatDirectoryObject)directoryStack.Peek();
            }
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        protected static extern bool GetDiskFreeSpace(string lpRootPathName,
           out uint lpSectorsPerCluster,
           out uint lpBytesPerSector,
           out uint lpNumberOfFreeClusters,
           out uint lpTotalNumberOfClusters);
    }
}
