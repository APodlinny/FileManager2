using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FileManager2.Model
{
    static class PartitionEnumerator
    {
        public static IEnumerable<IPartition> Enumerate()
        {
            int hddNumber = 0;
            var info = new HardDriveInfo(hddNumber);
            hddNumber++;

            if (info.Partitions != null)
            {
                foreach (var partition in info.Partitions)
                {
                    yield return partition;
                }
            }
            else
            {
                yield break;
            }
        }
    }
}
