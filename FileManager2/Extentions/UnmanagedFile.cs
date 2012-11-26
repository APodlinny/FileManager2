using System;
using System.IO;
using System.Runtime.InteropServices;

namespace FileManager2.Extentions
{
    /// <summary>
    /// Class gives an interface to unmanaged files.
    /// </summary>
    public class UnmanagedFile : IDisposable
    {
        private const short FileAttributeNormal = 0x80;
        private const short InvalidHandleValue = -1;
        private const uint GenericRead = 0x80000000;
        private const uint GenericWrite = 0x40000000;
        private const uint CreateNew = 1;
        private const uint CreateAlways = 2;
        private const uint OpenExisting = 3;
        private const uint FileShareRead = 1;
        private const uint FileShareWrite = 2;
        private const int InvalidSetFilePointer = -1;

        private IntPtr _handleValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnmanagedFile"/> class.
        /// </summary>
        /// <param name="path">
        /// The path.
        /// </param>
        public UnmanagedFile(string path)
        {
            Load(path);
        }

        /// <summary>
        /// Gets file handle.
        /// </summary>
        /// <exception cref="Exception">
        /// </exception>
        public IntPtr Handle
        {
            get
            {
                // If the handle is valid,
                // return it.
                if (_handleValue.ToInt32() != InvalidHandleValue)
                {
                    return _handleValue;
                }

                throw new Exception("Trying to use invalid file handle.");
            }
        }

        /// <summary>
        /// Creates an unmanaged file using specified path to it.
        /// </summary>
        /// <param name="path">
        /// The path.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public void Load(string path)
        {
            // Try to open the file.
            _handleValue = CreateFile(path, GenericRead, FileShareRead | FileShareWrite, 
                IntPtr.Zero, OpenExisting, 0, IntPtr.Zero);

            // If the handle is invalid,
            // get the last Win32 error 
            // and throw a Win32Exception.
            if (_handleValue.ToInt32() == InvalidHandleValue)
            {
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }
        }

        /// <summary>
        /// Method reads raw data from file.
        /// </summary>
        /// <param name="count">
        /// The count of bytes to read.
        /// </param>
        /// <returns>
        /// Returns raw data like a byte array.
        /// </returns>
        /// <exception cref="IOException">
        /// </exception>
        public byte[] ReadFile(uint count)
        {
            var buffer = new byte[count];
            uint bytesRead;

            if (ReadFile(_handleValue, buffer, count, out bytesRead, IntPtr.Zero))
            {
                var result = new byte[bytesRead];
                Array.Copy(buffer, result, bytesRead);
                return result;
            }
            
            Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            throw new IOException("Can't read from unmanaged file.");
        }

        /// <summary>
        /// Moves (sets) file pointer.
        /// </summary>
        /// <param name="position">
        /// The position (low part).
        /// </param>
        /// <param name="additonalPosition">
        /// The additonal position (optional high part).
        /// </param>
        public void MovePointer(int position, int additonalPosition = 0)
        {
            var lpDistanceToMoveHigh = additonalPosition;
            var result = SetFilePointer(_handleValue, position, out lpDistanceToMoveHigh, 0);

            if (result == InvalidSetFilePointer)
            {
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }
        }

        /// <summary>
        /// Closes unmanaged file.
        /// </summary>
        public void Dispose()
        {
            CloseHandle(_handleValue);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadFile(
            IntPtr hFile,
            [Out] byte[] lpBuffer,
            uint nNumberOfBytesToRead,
            out uint lpNumberOfBytesRead,
            IntPtr lpOverlapped);

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int SetFilePointer(
            [In] IntPtr hFile,
            [In] int lDistanceToMove,
            [Out] out int lpDistanceToMoveHigh,
            [In] int dwMoveMethod);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);
    }
}
