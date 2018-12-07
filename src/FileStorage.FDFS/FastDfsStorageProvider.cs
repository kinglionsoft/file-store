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
                    .Select(trackerIp =>
                    {
                        if (!IPAddress.TryParse(trackerIp, out var address))
                        {
                            address = Dns.GetHostAddresses(trackerIp).FirstOrDefault();
                            if (address == null)
                            {
                                throw new ArgumentException($"Can't get IP from {trackerIp}");
                            }
                        }
                        return new IPEndPoint(address, _option.TrackerPort);
                    })
                    .ToList());
            }
        }

        public async Task<string> UploadAsync(UploadFileModel model, CancellationToken token)
        {
            var storageNode = await FastDFSClient.GetStorageNodeAsync(model.GroupName);
            token.ThrowIfCancellationRequested();
            var fileId = await storageNode.UploadFileAsync(model.FileStream, model.Extension, token);
            return _option.FileUrlPrefix + '/' + storageNode.GroupName + '/' + fileId;
        }

        public Task DeleteFileAsync(string fileUrl, CancellationToken token)
        {
            return FastDFSClient.RemoveFileAsync(fileUrl, token);
        }

        public Task<QUERY_FILE_INFO_Result> GetFileInfo(string fileUrl)
        {
            return FastDFSClient.GetFileInfoAsync(fileUrl);
        }
    }
}
