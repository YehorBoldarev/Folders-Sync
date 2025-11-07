using System.Security.Cryptography;

namespace FolderSync.Source
{
    public class FileComparer: IEqualityComparer<FileInfo>
    {
        public static readonly FileComparer Instance = new FileComparer();
        private FileComparer() { }

        public bool Equals(FileInfo? x, FileInfo? y)
        {
            if (x!.Name != y!.Name || x.CreationTimeUtc != y.CreationTimeUtc || x.LastWriteTimeUtc != y.LastWriteTimeUtc || x.Length != y.Length)
                return false;

            if (!ComputeMD5(x).SequenceEqual(ComputeMD5(y)))
                return false;

            return true;
        }

        private byte[] ComputeMD5(FileInfo fileInfo)
        {
            using (MD5 md5 = MD5.Create())
            {
                using (FileStream fileStream = fileInfo.OpenRead())
                {
                    fileStream.Position = 0;
                    return md5.ComputeHash(fileStream);
                }
            }
        }

        public int GetHashCode(FileInfo obj)
        {
            return obj.FullName.GetHashCode();
        }
    }
}
