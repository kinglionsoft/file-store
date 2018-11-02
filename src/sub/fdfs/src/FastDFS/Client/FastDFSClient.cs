using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace FastDFS.Client
{
    public static class FastDFSClient
    {
        public static async Task<QUERY_FILE_INFO_Result> GetFileInfoAsync(string fileUrl)
        {
            var (groupName, fileName) = SplitFileId(fileUrl);

            var storageNode = await GetStorageNodeAsync(groupName);

            return await new QUERY_FILE_INFO(storageNode.EndPoint)
                .RequestAsync(new QUERY_FILE_INFO_Args(storageNode, fileName),
                    CancellationToken.None);
        }

        public static async Task<StorageNode> GetStorageNodeAsync(string groupName)
        {
            var response = await new QUERY_STORE_WITH_GROUP_ONE()
                    .RequestAsync(new QUERY_STORE_WITH_GROUP_ONE_Args(groupName),
                                  CancellationToken.None);

            var point = new IPEndPoint(IPAddress.Parse(response.IPStr), response.Port);
            return new StorageNode
            {
                GroupName = response.GroupName,
                EndPoint = point,
                StorePathIndex = response.StorePathIndex
            };
        }

        public static async Task RemoveFileAsync(string groupName, string fileName, CancellationToken token)
        {
            var response = await new QUERY_UPDATE()
                .RequestAsync(new QUERY_UPDATE_Args(groupName, fileName), token);

            var point = new IPEndPoint(IPAddress.Parse(response.IPStr), response.Port);
            await new DELETE_FILE(point).RequestAsync(new DELETE_FILE_Args(groupName, fileName), token);
        }

        public static Task RemoveFileAsync(string fileUrl, CancellationToken token)
        {
            var (groupName, fileName) = SplitFileId(fileUrl);
            return RemoveFileAsync(groupName, fileName, token);
        }

        public static async Task<string> UploadFileAsync(this StorageNode storageNode, Stream fileStream, string fileExt, CancellationToken token)
        {
            var response = await new UPLOAD_FILE(storageNode.EndPoint).RequestAsync(
                    new UPLOAD_FILE_Args(storageNode, fileStream, fileExt), 
                    token);
            return response.FileName;
        }

        public static async Task<string> UploadSlaveFileAsync(this StorageNode storageNode, 
            string masterFile,
            string prefix,
            Stream fileStream, 
            string fileExt,
            CancellationToken token)
        {
            var response = await new UPLOAD_SLAVE_FILE(storageNode.EndPoint).RequestAsync(
                new UPLOAD_SLAVE_FILE_Args(masterFile, prefix, fileExt, fileStream),
                token);
            return response.FileName;
        }

        private static (string GroupName, string FileName) SplitFileId(string fileUrl)
        {
            var uri = new Uri(fileUrl, UriKind.RelativeOrAbsolute);
            if (uri.IsAbsoluteUri)
            {
                fileUrl = uri.LocalPath;
            }
            var startIndex = fileUrl.StartsWith("/") ? 1 : 0;
            var fileNameStartIndex = fileUrl.IndexOf('/', startIndex);
            var groupName = fileUrl.Substring(startIndex, fileNameStartIndex - 1);
            var fileName = fileUrl.Substring(fileNameStartIndex + 1);
            return (groupName, fileName);
        }
    }
}