using System;
using System.Threading;
using System.Threading.Tasks;

namespace FileStorage.SDK.Client
{
    public static class FileStorageExtensions
    {
        public static Task<ApiResult<string[]>> UploadAsync(this IFileStorage storage, string file)
        {
            return storage.UploadAsync(new[] { file });
        }

        public static async Task<bool> TryDeleteAsync(this IFileStorage storage, string fileUrl,
            CancellationToken token = default)
        {
            try
            {
                var result = await storage.DeleteAsync(fileUrl, token);
                return result != null && result.Success;
            }
            catch
            {
                return false;
            }
        }
    }
}