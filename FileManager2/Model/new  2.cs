using System;

namespace Exam
{
	enum DirectoryObjectType
    {
        File, Directory
    }
	
	public class DirectoryObject
    {
        public string Name { get; set; }
        public DirectoryObjectType Type { get; set; }
        public int Size { get; set; }
		public bool isSystem { get; set; }
    }

	class Program
	{
		UnmanagedFile diskFile;
		string diskFileName;
		string rootDirectoryName;
		int rootDirectorySector;
		int rootDirectoryEntities;
		uint bytesPerSector;
		int bitsPerSector;
		uint bytesPerCluster;
		byte sectorsPerCluster;

		int fatCopiesNumber;
		int fatSize;
		int reservedAreaSize;
		byte[] fatContent;
		int fatAddress;
		uint fatContentSize;
		
		List<DirectoryObject> contents;
	
		public static void Main()
		{
			Initialize();
			
			var shortestSystemFiles = contents.
				Where(o => o.Type == DirectoryObjectType.File).
				Where(f => f.isSystem).
				OrderBy(f => f.Size);
				
			Console.WriteLine("Самые короткие системные файлы по возрастанию размера:");
			shortestSystemFiles.
				ToList().
				ForEach(f => Console.WriteLine("Имя: {0}; Размер: {1}", f.Name, f.Size));
		}
		
		void Initialize()
		{
			int hddNumber = ...;
			int partitionAddress = ...;
			
			string hardDriveName = "\\\\.\\PHYSICALDRIVE" + hddNumber;
			diskFile = new UnmanagedFile(hardDriveName);
			bytesPerSector = GetBytesPerSector();
			bitsPerSector = GetBitsNumber(bytesPerSector);
			diskFile.MovePointer(partitionAddress << bitsPerSector,
								 partitionAddress >> (32 - bitsPerSector));
								 
			byte[] fatInfo = diskFile.ReadFile(bytesPerSector);
			fatCopiesNumber = fatInfo.AsWord(16);
            fatSize = fatInfo.AsWord(22);
            reservedAreaSize = fatInfo.AsWord(14);
            sectorsPerCluster = fatInfo.AsByte(13);
            fatAddress = reservedAreaSize;
            rootDirectoryEntities = fatInfo.AsWord(17);
			rootDirectorySector = reservedAreaSize + (fatCopiesNumber * fatSize);
			
			bytesPerCluster = bytesPerSector * sectorsPerCluster;
            fatContentSize = (uint)(fatSize * bytesPerSector);
            fatContent = LoadFatContent();
			
			contents = LoadCurrentDirectoryObjects().ToList();
		}
		
		IEnumerable<DirectoryObject> LoadCurrentDirectoryObjects()
		{
			var objectBuffers = GetSectorObjectBuffers(rootDirectorySector);
				
			return DirectoryObjectBufferProcessor.
                Process(objectBuffers).
                Where(x => (x.Name != "..") && (x.Name != "."));
		}
		
		uint GetBytesPerSector()
        {
            uint sectorsPerCluster;
            uint lpBytesPerSector;
            uint numberOfFreeClusters;
            uint totalNumberOfClusters;

            GetDiskFreeSpace(rootDirectoryName, out sectorsPerCluster, out lpBytesPerSector,
               out numberOfFreeClusters, out totalNumberOfClusters);

            return lpBytesPerSector;
        }
		
		int GetBitsNumber(uint x)
        {
            uint i = x;
            int n = 0;

            while ((i = i >> 1) != 0)
            {
                n++;
            }

            return n;
        }
		
		byte[] LoadFatContent()
        {
            diskFile.MovePointer(fatAddress << bitsPerSector);

            return diskFile.ReadFile(fatContentSize);
        }
		
		IEnumerable<byte[]> GetSectorObjectBuffers(int sector)
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
		
		IEnumerable<int> GetObjectClusters(int cluster)
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
		
		int GetNextCluster(int cluster)
        {
            return fatContent.AsInt32(cluster * 2);
        }
		
		IsLastCluster(int cluster)
        {
            return (uint)cluster >= 0xFFF8;
        }
	}
	
	class DirectoryObjectBufferProcessor
    {
		public static IEnumerable<Model.FatDirectoryObject> Process(IEnumerable<byte[]> bufs)
        {
			var buffers = bufs.ToList();

            for (int i = 0; i < buffers.Count; i++)
            {
                if (HasLongName(buffers[i]))
                {
                    int partsNumber = buffers[i][0] & 0xBF;

                    var nameParts = Enumerable.Range(i, partsNumber).
                        Select(n => GetLongNamePart(buffers[n])).
                        Reverse();

                    i += partsNumber;

                    string name = String.Join(String.Empty, nameParts);
                    var type = (buffers[i][11] & 0x10) != 0 ?
                        Model.DirectoryObjectType.Directory :
                        Model.DirectoryObjectType.File;

                    var size = buffers[i].AsInt32(28);
					var sysAttr = buffers[i][11] & 0x04;

                    yield return new Model.FatDirectoryObject
                    {
                        Name = name,
                        Type = type,
                        Size = size,
						isSystem = sysAttr != 0
                    };
                }
                else
                {
                    var name = GetShorName(buffers[i]);
                    var type = (buffers[i][11] & 0x10) != 0 ?
                        Model.DirectoryObjectType.Directory :
                        Model.DirectoryObjectType.File;

                    var size = buffers[i].AsInt32(28);
					var sysAttr = buffers[i][11] & 0x04;

                    yield return new Model.FatDirectoryObject
                    {
                        Name = name,
                        Type = type,
                        Size = size,
						isSystem = sysAttr != 0
                    };
                }
            }

            yield break;
		}
		
		static bool HasLongName(byte[] buffer)
        {
            return buffer[11] == 0x0F;
        }
		
		static string GetLongNamePart(byte[] buffer)
        {
            byte[] firstPart = Enumerable.Range(1, 10).Select(i => buffer[i]).ToArray();
            byte[] secondPart = Enumerable.Range(14, 12).Select(i => buffer[i]).ToArray();
            byte[] thirdPart = Enumerable.Range(28, 4).Select(i => buffer[i]).ToArray();

            byte[] fullName = firstPart.Concat(secondPart).Concat(thirdPart).ToArray();

            return BufferToUnicode(fullName);
        }
		
		static string GetShorName(byte[] buffer)
        {
            var nameLetters = Enumerable.Range(0, 8).Select(i => ((char)buffer[i]).ToString());
            var extensionLetters = Enumerable.Range(8, 3).Select(i => ((char)buffer[i]).ToString());

            var name = String.Join(String.Empty, nameLetters);
            var extension = String.Join(String.Empty, extensionLetters);

            int nameLastLetter = GetLastLetterIndex(name);
            int extensionLastLetter = GetLastLetterIndex(extension);

            name = name.Remove(nameLastLetter + 1, 7 - nameLastLetter);
            extension = extension.Remove(extensionLastLetter + 1, 2 - extensionLastLetter);

            if (extension.Length == 0)
            {
                return name;
            }

            return String.Format("{0}.{1}", name, extension);
        }
		
		static int GetLastLetterIndex(string name)
        {
            int index;
            for (index = name.Length - 1; index > -1; index--)
            {
                if (name[index] != ' ')
                {
                    break;
                }
            }

            return index;
        }
	}
}