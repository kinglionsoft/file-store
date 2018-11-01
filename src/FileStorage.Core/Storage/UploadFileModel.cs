using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FileStorage.Core
{
    public class UploadFileModel
    {
        private readonly Stream _fileStream;
        public readonly string Extension;
        public readonly string GroupName;

        public UploadFileModel(Stream fileStream, string extension, string groupName)
        {
            _fileStream = fileStream;
            Extension = extension.TrimStart('.');
            GroupName = groupName;
        }

        public Task<byte[]> GetFileDataAsync(CancellationToken token)
            => _fileStream.ReadToEndAsync(token);
    }
}
