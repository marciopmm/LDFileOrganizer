using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LDFileOrganizer.Analyzers
{
    public class DefaultFileComparer : IEqualityComparer<FileInfo>
    {
        public bool Equals(FileInfo x, FileInfo y)
        {
            // If length is equal, compare by file hash
            if (x.Length == y.Length)
            {
                return FileToSHA256Hash(x).Equals(FileToSHA256Hash(y));
            }

            return false;
        }

        public int GetHashCode(FileInfo obj)
        {
            return obj.GetHashCode();
        }

        private string FileToSHA256Hash(FileInfo file)
        {
            using (var stream = file.OpenRead())
            {
                SHA256Managed sha = new SHA256Managed();
                byte[] checksum = sha.ComputeHash(stream);
                return BitConverter.ToString(checksum).Replace("-", string.Empty);
            }
        }
    }
}
