using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FileStorage.Core;

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

        public static async Task DownloadToFileAsync(this IFileStorage storage, string output, FilesDownloadModel input,
            CancellationToken token = default)
        {
            using (var stream = await storage.DownloadAsync(input, token))
            {
                using (var writer = File.Open(output, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    writer.SetLength(0);
                    writer.Seek(0, SeekOrigin.Begin);
                    await stream.CopyToAsync(writer, 4096, token);
                }
            }
        }
    }
}