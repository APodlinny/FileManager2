namespace FileManager2.Model
{
    /// <summary>
    /// Enum for different types of file system.
    /// </summary>
    public enum FileSystemType
    {
        /// <summary>
        /// FAT16 file system.
        /// </summary>
        Fat16,

        /// <summary>
        /// FAT32 file system.
        /// </summary>
        Fat32,

        /// <summary>
        /// NTFS file system.
        /// </summary>
        Ntfs,

        /// <summary>
        /// Unknown file system.
        /// </summary>
        Unknown
    }

    /// <summary>
    /// Structure with some usefull partition information.
    /// </summary>
    public class PartitionInfo
    {
        /// <summary>
        /// Shows whether partition is active.
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// Shows whether partition is extended.
        /// </summary>
        public bool Extended { get; set; }

        /// <summary>
        /// Partition letter.
        /// </summary>
        public char Letter { get; set; }

        /// <summary>
        /// Partition file system.
        /// </summary>
        public FileSystemType FileSystem { get; set; }

        /// <summary>
        /// Number of hard disk drive.
        /// </summary>
        public int HddNumber { get; set; }

        /// <summary>
        /// Partition address.
        /// </summary>
        public int Address { get; set; }

        /// <summary>
        /// Partition size in bytes.
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// Free space in bytes.
        /// </summary>
        public int FreeSpace { get; set; }

        /// <summary>
        /// Disk serial number.
        /// </summary>
        public byte[] SerialNumber { get; set; }
    }
}