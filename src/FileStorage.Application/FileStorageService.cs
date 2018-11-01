using System;
using System.Threading;
using System.Threading.Tasks;
using FileStorage.Core;

namespace FileStorage.Application
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IStorageProvider _storageProvider;

        public FileStorageService(IStorageProvider storageProvider)
        {
            _storageProvider = storageProvider;
        }

        public Task<string> UploadAsync(UploadFileModel model, CancellationToken token = default)
        {
            return _storageProvider.UploadAsync(model, token);
        }

        public Task DeleteAsync(string url, CancellationToken token = default)
        {
            return _storageProvider.DeleteFileAsync(url, token);
        }
    }
}
