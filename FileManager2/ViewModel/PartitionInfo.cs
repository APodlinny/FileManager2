using System;
using System.Linq;
using FileManager2;

namespace FileManager2.ViewModel
{
    public class PartitionInfo : Model.PartitionInfo
    {
        public PartitionInfo(Model.PartitionInfo info)
        {
            Active = info.Active;
            Extended = info.Extended;
            Letter = info.Letter;
            FileSystem = info.FileSystem;
            HddNumber = info.HddNumber;
            Address = info.Address;
            Size = info.Size;
            FreeSpace = info.FreeSpace;
            SerialNumber = info.SerialNumber;
        }

        public string StringSerialNumber
        {
            get { return string.Join("-", SerialNumber.Select(x => x.ToString("X2"))); }
            set { }
        }

        public string StringSize
        {
            get { return Size > 1024 ? Math.Round(Size / 1024.0, 3) + " GB" : Size + " MB"; }
            set { }
        }

        public string StringFreeSpace
        {
            get
            {
                return FreeSpace == -1
                           ? "Unknown"
                           : FreeSpace > 1024
                                 ? Math.Round(FreeSpace / 1024.0, 3) + " GB"
                                 : FreeSpace + " MB";
            }

            set { }
        }
    }
}
