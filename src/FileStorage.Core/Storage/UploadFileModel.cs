using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FileStorage.Core
{
    public class UploadFileModel
    {
        public readonly Stream FileStream;
        public readonly string Extension;
        public readonly string GroupName;

        public UploadFileModel(Stream fileStream, string extension, string groupName)
        {
            Extension = extension.TrimStart('.');
            if (Extension.Length > 6)
            {
                throw new Exception("file ext is too long");
            }
            FileStream = fileStream;
            GroupName = groupName;
        }

        public Task<byte[]> GetFileDataAsync(CancellationToken token)
            => FileStream.ReadToEndAsync(token);
    }
}
