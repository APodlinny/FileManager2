using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FileManager2.Extentions;
using System.Runtime.InteropServices;

namespace FileManager2.Model
{
    class HardDriveInfo
    {
        private UnmanagedFile _handle;
        private int _number;
        private int _sectorSize;
        private int _bitsPerSector;
        private int _extendedAreaAddress;

        public HardDriveInfo(int hddNumber)
        {
            _number = hddNumber;
            _sectorSize = 0;
            _extendedAreaAddress = 0;
            string hardDriveName = "\\\\.\\PHYSICALDRIVE" + hddNumber;

            try
            {
                _handle = new UnmanagedFile(hardDriveName);
                byte[] mbr = ReadMbr();
                if (mbr.AsWord(0x01fe) != 0xAA55)
                {
                    throw new ErrorException("Partition is broken or not valid.");
                }

                var partitions = GetHddPartitions(mbr);
                if (_extendedAreaAddress != 0)
                {
                    int ext = _extendedAreaAddress;
                    _extendedAreaAddress = 0;

                    while (true)
                    {
                        int nextMbrAddress = ext + _extendedAreaAddress;

                        _handle.MovePointer(nextMbrAddress << _bitsPerSector, nextMbrAddress >> (32 - _bitsPerSector));
                        mbr = ReadMbr();
                        if (mbr.AsWord(0x01fe) != 0xAA55)
                        {
                            throw new ErrorException("Partition is broken or not valid.");
                        }

                        partitions.Concat(GetHddPartitions(mbr, nextMbrAddress));

                        if (_extendedAreaAddress == 0)
                        {
                            break;
                        }
                    }
                }

                Partitions = partitions;
            }
            catch (System.IO.FileNotFoundException)
            {
                Partitions = null;
            }

            
        }

        public IEnumerable<IPartition> Partitions { get; set; }

        private enum DriveType : uint
        {
            /// <summary>
            /// The drive type cannot be determined.
            /// </summary>
            Unknown = 0, // DRIVE_UNKNOWN

            /// <summary>
            /// The root path is invalid, for example, no volume is mounted at the path.
            /// </summary>
            Error = 1,        // DRIVE_NO_ROOT_DIR

            /// <summary>
            /// The drive is a type that has removable media, for example, a floppy drive or removable hard disk.
            /// </summary>
            Removable = 2,    // DRIVE_REMOVABLE

            /// <summary>
            /// The drive is a type that cannot be removed, for example, a fixed hard drive.
            /// </summary>
            Fixed = 3,        // DRIVE_FIXED

            /// <summary>
            /// The drive is a remote (network) drive.
            /// </summary>
            Remote = 4,        // DRIVE_REMOTE

            /// <summary>
            /// The drive is a CD-ROM drive.
            /// </summary>
            Cdrom = 5,        // DRIVE_CDROM

            /// <summary>
            /// The drive is a RAM disk.
            /// </summary>
            RamDisk = 6        // DRIVE_RAMDISK
        }

        [Flags]
        private enum FileSystemFeature : uint
        {
            /// <summary>
            /// The file system supports case-sensitive file names.
            /// </summary>
            CaseSensitiveSearch = 1,

            /// <summary>
            /// The file system preserves the case of file names when it places a name on disk.
            /// </summary>
            CasePreservedNames = 2,

            /// <summary>
            /// The file system supports Unicode in file names as they appear on disk.
            /// </summary>
            UnicodeOnDisk = 4,

            /// <summary>
            /// The file system preserves and enforces access control lists (ACL).
            /// </summary>
            PersistentAcls = 8,

            /// <summary>
            /// The file system supports file-based compression.
            /// </summary>
            FileCompression = 0x10,

            /// <summary>
            /// The file system supports disk quotas.
            /// </summary>
            VolumeQuotas = 0x20,

            /// <summary>
            /// The file system supports sparse files.
            /// </summary>
            SupportsSparseFiles = 0x40,

            /// <summary>
            /// The file system supports re-parse points.
            /// </summary>
            SupportsReparsePoints = 0x80,

            /// <summary>
            /// The specified volume is a compressed volume, for example, a DoubleSpace volume.
            /// </summary>
            VolumeIsCompressed = 0x8000,

            /// <summary>
            /// The file system supports object identifiers.
            /// </summary>
            SupportsObjectIDs = 0x10000,

            /// <summary>
            /// The file system supports the Encrypted File System (EFS).
            /// </summary>
            SupportsEncryption = 0x20000,

            /// <summary>
            /// The file system supports named streams.
            /// </summary>
            NamedStreams = 0x40000,

            /// <summary>
            /// The specified volume is read-only.
            /// </summary>
            ReadOnlyVolume = 0x80000,

            /// <summary>
            /// The volume supports a single sequential write.
            /// </summary>
            SequentialWriteOnce = 0x100000,

            /// <summary>
            /// The volume supports transactions.
            /// </summary>
            SupportsTransactions = 0x200000,
        }

        [DllImport("kernel32.dll")]
        private static extern uint GetLogicalDrives();

        [DllImport("kernel32.dll")]
        private static extern DriveType GetDriveType([MarshalAs(UnmanagedType.LPStr)] string lpRootPathName);

        [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool GetVolumeInformation(
          string rootPathName,
          StringBuilder volumeNameBuffer,
          int volumeNameSize,
          out uint volumeSerialNumber,
          out uint maximumComponentLength,
          out FileSystemFeature fileSystemFlags,
          StringBuilder fileSystemNameBuffer,
          int nFileSystemNameSize);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool GetDiskFreeSpace(string lpRootPathName,
           out uint lpSectorsPerCluster,
           out uint lpBytesPerSector,
           out uint lpNumberOfFreeClusters,
           out uint lpTotalNumberOfClusters);

        private IEnumerable<IPartition> GetHddPartitions(byte[] mbr, int additionalOffset = 0)
        {
            int partitonOffset = 0x1be;
            _extendedAreaAddress = 0;

            for (int i = 0; i < 4; i++, partitonOffset += 0x10)
            {
                var info = new PartitionInfo();
                info.HddNumber = _number;
                info.Extended = additionalOffset != 0;

                int id = mbr.AsByte(partitonOffset + 4);
                if (id == 0)
                {
                    break;
                }

                info.Active = mbr.AsByte(partitonOffset + 0) != 0;

                int startAddress = mbr.AsInt32(partitonOffset + 8);
                if ((id == 5) || (id == 15))
                {
                    _extendedAreaAddress = startAddress;
                    continue;
                }

                int sizeInSectors = mbr.AsInt32(partitonOffset + 12);
                info.Size = sizeInSectors / 2048 * _sectorSize / 512;
                info.FreeSpace = -1;

                int absoluteAddress = startAddress + additionalOffset;
                info.SerialNumber = GetDiskSn(absoluteAddress, id);
                info.Letter = GetDiskLetter(info.SerialNumber);
                info.Address = absoluteAddress;

                if (info.Letter == '?')
                {
                    // disks.Add(new UnknownDisk(info));
                    info.FileSystem = FileSystemType.Unknown;
                }

                switch (id)
                {
                    case 0x01:
                    case 0x04:
                    case 0x06:
                    case 0x0E:
                        {
                            // disks.Add(new Fat16Disk(info));
                            info.FileSystem = FileSystemType.Fat16;
                            break;
                        }

                    case 0x07:
                    case 0x17:
                        {
                            // disks.Add(new NtfsDisk(info));
                            info.FileSystem = FileSystemType.Ntfs;
                            break;
                        }

                    case 0x0B:
                    case 0x0C:
                    case 0x1B:
                    case 0x1C:
                    case 0x8B:
                    case 0x8C:
                        {
                            // disks.Add(new Fat32Disk(info));
                            info.FileSystem = FileSystemType.Fat32;
                            break;
                        }

                    default:
                        {
                            // disks.Add(new UnknownDisk(info));
                            info.FileSystem = FileSystemType.Unknown;
                            break;
                        }
                }

                var partitionFactory = new PartitionFactory(info);
                yield return partitionFactory.Partition;
            }

            yield break;
        }

        private byte[] GetDiskSn(int absoluteAddress, int id)
        {
            _handle.MovePointer(absoluteAddress << _bitsPerSector,
                               absoluteAddress >> (32 - _bitsPerSector));

            byte[] buffer = ReadMbr();

            switch (id)
            {
                case 0x07:
                    {
                        return Enumerable.Range(72, 8).
                            Select(i => buffer.AsByte(i)).
                            ToArray();
                    }

                case 0x0e:
                case 0x0c:
                case 0x0b:
                    {
                        return Enumerable.Range(67, 4).
                            Select(i => buffer.AsByte(i)).
                            ToArray();
                    }

                case 0x01:
                case 0x04:
                case 0x06:
                case 0x0d:
                    {
                        return Enumerable.Range(39, 4).
                            Select(i => buffer.AsByte(i)).
                            ToArray();
                    }

                default:
                    {
                        return null;
                    }
            }
        }

        private char GetDiskLetter(byte[] serialNumber)
        {
            if (serialNumber != null)
            {
                for (int i = 2; i < 26; i++)
                {
                    if ((GetLogicalDrives() & (1 << i)) != 0)
                    {
                        string diskName = ((char)('A' + i)) + ":\\";
                        if (GetDriveType(diskName) != DriveType.Cdrom)
                        {
                            if (VolumeSN(diskName) == serialNumber.AsInt32(0))
                            {
                                return diskName[0];
                            }
                        }
                    }
                }
            }

            return '?';
        }

        private int VolumeSN(string volumeName)
        {
            uint serialNumber;
            uint uselessVar;
            FileSystemFeature uselessFlags;

            GetVolumeInformation(volumeName, null, 0, out serialNumber, out uselessVar, out uselessFlags, null, 0);

            return (int)serialNumber;
        }

        private byte[] ReadMbr()
        {
            int size = _sectorSize != 0 ? _sectorSize : 0x200;

            var buffer = _handle.ReadFile((uint)size);

            if (size != _sectorSize)
            {
                int i = size;
                int n = 0;
                while ((i = i / 2) != 0)
                {
                    n++;
                }

                _bitsPerSector = n;
                _sectorSize = size;
            }

            return buffer;
        }
    }
}
