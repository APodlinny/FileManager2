namespace FileManager2.Model
{
    /// <summary>
    /// Enum that represents different types of directory object.
    /// </summary>
    public enum DirectoryObjectType
    {
        /// <summary>
        /// Says that objects is a file.
        /// </summary>
        File,

        /// <summary>
        /// Says that object is a directory.
        /// </summary>
        Directory
    }

    /// <summary>
    /// Class represents an object of the directory.
    /// </summary>
    public class DirectoryObject
    {
        /// <summary>
        /// Gets or sets name of the object.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets type of the object.
        /// </summary>
        public DirectoryObjectType Type { get; set; }

        /// <summary>
        /// Size in bytes.
        /// </summary>
        public int Size { get; set; }
    }
}
