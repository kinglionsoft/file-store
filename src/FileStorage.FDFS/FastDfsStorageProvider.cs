using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FastDFS.Client;
using FileStorage.Core;
using Microsoft.Extensions.Options;

namespace FileStorage.FDFS
{
    public class FastDfsStorageProvider : IStorageProvider
    {
        private readonly FastDfsOption _option;

        private static readonly object _initLock = new object();

        public FastDfsStorageProvider(IOptions<FastDfsOption> optionAccessor)
        {
            _option = optionAccessor.Value;

            lock (_initLock)
            {
                ConnectionManager.Initialize(_option
                    .TrackerIps
                    .Select(trackerIp => new IPEndPoint(IPAddress.Parse(trackerIp), _option.TrackerPort))
                    .ToList());
            }
        }

        public async Task<string> UploadAsync(UploadFileModel model, CancellationToken token)
        {
            StorageNode storageNode = await FastDFSClient.GetStorageNodeAsync(model.GroupName);
            token.ThrowIfCancellationRequested();
            //var fileId = await FastDFSClient.UploadFileAsync(storageNode,
            //    await model.GetFileDataAsync(token), model.Extension);
            var fileId = await storageNode.UploadFileAsync(model.FileStream, model.Extension, token);
            return _option.FileUrlPrefix + '/' + storageNode.GroupName + '/' + fileId;
        }

        public Task DeleteFileAsync(string fileUrl, CancellationToken token)
        {
            fileUrl = new Uri(fileUrl).LocalPath;
            var startIndex = fileUrl.StartsWith("/") ? 1 : 0;
            var fileNameStartIndex = fileUrl.IndexOf('/', startIndex);
            var groupName = fileUrl.Substring(startIndex, fileNameStartIndex - 1);
            var fileName = fileUrl.Substring(fileNameStartIndex + 1);
            token.ThrowIfCancellationRequested();
            return FastDFSClient.RemoveFileAsync(groupName, fileName);
        }
    }
}
