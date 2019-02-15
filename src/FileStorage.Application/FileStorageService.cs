using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using FileStorage.Core;

namespace FileStorage.Application
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IStorageProvider _storageProvider;
        private readonly IFastDfsHttpClient _fastDfsHttpClient;

        public FileStorageService(IStorageProvider storageProvider, IFastDfsHttpClient fastDfsHttpClient)
        {
            _storageProvider = storageProvider;
            _fastDfsHttpClient = fastDfsHttpClient;
        }

        public Task<string> UploadAsync(UploadFileModel model, CancellationToken token = default)
        {
            return _storageProvider.UploadAsync(model, token);
        }

        public Task DeleteAsync(string url, CancellationToken token = default)
        {
            return _storageProvider.DeleteFileAsync(url, token);
        }

        /// <summary>
        /// 批量打包下载
        /// </summary>
        /// <param name="input">{"path": "url"}</param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<string> DownloadAsync(Dictionary<string, string> input, CancellationToken token = default)
        {
            var tmpFile = Path.GetTempFileName();
            using (var zip =  ZipFile.Open(tmpFile, ZipArchiveMode.Update))
            {
                foreach (var file in input)
                {
                    var entry = zip.CreateEntry(file.Key);
                    var stream = await _fastDfsHttpClient.DownloadAsync(file.Value, token);
                    await stream.CopyToAsync(entry.Open());
                }
            }

            return tmpFile;
        }
    }
}
