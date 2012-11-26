using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FileManager2.Model
{
    class PartitionFactory
    {
        private IPartition _partition;

        /// <summary>
        /// Initializes a new instance of the <see cref="PartitionFactory"/> class.
        /// </summary>
        /// <param name="info">
        /// The partition info.
        /// </param>
        public PartitionFactory(PartitionInfo info)
        {
            if (info.Letter == '?')
            {
                _partition = new Fat16Partition(info);
            }
            else
            {
                switch (info.FileSystem)
                {
                    case FileSystemType.Fat16:
                        _partition = new Fat16Partition(info);
                        break;

                    case FileSystemType.Fat32:
                        _partition = new Fat32Partition(info);
                        break;

                    case FileSystemType.Ntfs:
                        _partition = new NtfsPartition(info);
                        break;

                    case FileSystemType.Unknown:
                        _partition = new Fat16Partition(info);
                        break;

                    default:
                        _partition = new Fat16Partition(info);
                        break;
                }

            }
        }

        /// <summary>
        /// Gets disk.
        /// </summary>
        public IPartition Partition
        {
            get { return _partition; }
        }
    }
}
