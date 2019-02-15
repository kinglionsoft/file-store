using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FileStorage.Core;
using JetBrains.Annotations;

namespace FileStorage.Application
{
    public interface IFileStorageService
    {
        Task DeleteAsync([NotNull]string url, CancellationToken token = default);
        Task<string> UploadAsync([NotNull]UploadFileModel model, CancellationToken token = default);
        Task<string> DownloadAsync([NotNull]Dictionary<string, string> input, CancellationToken token = default);
    }
}