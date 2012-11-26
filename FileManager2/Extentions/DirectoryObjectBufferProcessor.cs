using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FileManager2.Extentions
{
    public static class DirectoryObjectBufferProcessor
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

                    int cluster = GetObjectCluster(buffers[i]);

                    yield return new Model.FatDirectoryObject
                    {
                        Name = name,
                        Type = type,
                        Cluster = cluster,
                        Size = size
                    };
                }
                else
                {
                    var name = GetShorName(buffers[i]);
                    var type = (buffers[i][11] & 0x10) != 0 ?
                        Model.DirectoryObjectType.Directory :
                        Model.DirectoryObjectType.File;

                    int cluster = GetObjectCluster(buffers[i]);
                    var size = buffers[i].AsInt32(28);

                    yield return new Model.FatDirectoryObject
                    {
                        Name = name,
                        Type = type,
                        Cluster = cluster,
                        Size = size
                    };
                }
            }

            yield break;
        }

        private static bool HasLongName(byte[] buffer)
        {
            return buffer[11] == 0x0F;
        }

        private static int GetObjectCluster(byte[] buffer)
        {
            return (buffer.AsWord(20) << 16) | buffer.AsWord(26);
        }

        private static string GetLongNamePart(byte[] buffer)
        {
            byte[] firstPart = Enumerable.Range(1, 10).Select(i => buffer[i]).ToArray();
            byte[] secondPart = Enumerable.Range(14, 12).Select(i => buffer[i]).ToArray();
            byte[] thirdPart = Enumerable.Range(28, 4).Select(i => buffer[i]).ToArray();

            byte[] fullName = firstPart.Concat(secondPart).Concat(thirdPart).ToArray();

            return BufferToUnicode(fullName);
        }

        private static string GetShorName(byte[] buffer)
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

        private static int GetLastLetterIndex(string name)
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

        private static string BufferToUnicode(byte[] buffer)
        {
            string result = String.Empty;

            for (int i = 0;
                (i < buffer.Length) && ((buffer[i] != 0) || (buffer[i + 1] != 0));
                i += 2)
            {
                var wideChar = (ushort)((buffer[i + 1] << 8) | buffer[i]);
                result += ((char)wideChar).ToString();
            }

            return result;
        }
    }
}
