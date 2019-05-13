using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FileStorage.SDK.Client
{
    public interface IFileStorage
    {
        /// <summary>
        /// 上传文件压缩包
        /// </summary>
        /// <param name="files">文件</param>
        /// <param name="defaultExtension">若文件流中无法获取后缀名，则使用默认后缀名</param>
        /// <param name="groupName">文件组</param>
        /// <returns></returns>
        Task<ApiResult<string[]>> UploadPackageAsync(IEnumerable<string> files,
            string defaultExtension = null,
            string groupName = null,
            CancellationToken token = default);

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="files">文件</param>
        /// <param name="defaultExtension">若文件流中无法获取后缀名，则使用默认后缀名</param>
        /// <param name="groupName">文件组</param>
        /// <returns></returns>
        Task<ApiResult<string[]>> UploadAsync(IEnumerable<string> files,
            string defaultExtension = null,
            string groupName = null,
            CancellationToken token = default);

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="fileUrl">文件地址</param>
        Task<ApiResult> DeleteAsync(string fileUrl,
            CancellationToken token = default);

        /// <summary>
        /// 批量打包下载
        /// </summary>
        Task<Stream> DownloadAsync(FilesDownloadModel input, CancellationToken token = default);
    }
}