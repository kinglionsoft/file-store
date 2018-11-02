using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace FastDFS.Client
{
    public static class FastDFSClient
    {
        public static async Task<FDFSFileInfo> GetFileInfoAsync(this StorageNode storageNode, string fileName)
        {
            try
            {
                var req = QUERY_FILE_INFO.Instance.GetRequest(storageNode.EndPoint,
                    storageNode.GroupName,
                    fileName);

                return new FDFSFileInfo(await req.GetResponseAsync());
            }
            catch (FDFSStatusException)
            {
                return null;
            }
        }

        public static async Task<StorageNode> GetStorageNodeAsync(string groupName)
        {
            var responseBuffer =
                string.IsNullOrEmpty(groupName)
                    ? await QUERY_STORE_WITHOUT_GROUP_ONE.Instance.GetRequest().GetResponseAsync()
                    : await QUERY_STORE_WITH_GROUP_ONE.Instance.GetRequest(groupName).GetResponseAsync();

            var response = new QUERY_STORE_RESPONSE(responseBuffer);
            var point = new IPEndPoint(IPAddress.Parse(response.IPStr), response.Port);
            return new StorageNode
            {
                GroupName = response.GroupName,
                EndPoint = point,
                StorePathIndex = response.StorePathIndex
            };
        }

        public static async Task RemoveFileAsync(string groupName, string fileName)
        {
            var buffer = await QUERY_UPDATE.Instance.GetRequest(groupName, fileName).GetResponseAsync();
            var response = new QUERY_UPDATE.Response(buffer);
            var point = new IPEndPoint(IPAddress.Parse(response.IPStr), response.Port);
            await DELETE_FILE.Instance.GetRequest(point, groupName, fileName).GetResponseAsync();
        }

        public static async Task<string> UploadFileAsync(this StorageNode storageNode, Stream fileStream, string fileExt, CancellationToken token)
        {
            var response = await new UPLOAD_FILE(storageNode.EndPoint).RequestAsync(
                    new UploadArgs(storageNode, fileStream, fileExt),
                    token);
            return response.FileName;
        }
    }
}