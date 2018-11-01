using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace FileStorage.Core
{
    public interface IStorageProvider
    {
        [NotNull]
        Task<string> UploadAsync([NotNull]UploadFileModel model, CancellationToken token);

        Task DeleteFileAsync([NotNull]string fileUrl, CancellationToken token);
    }
}