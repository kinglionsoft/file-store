
namespace FileStorageApi.Controllers.Dto
{
    /// <summary>
    /// 压缩包上传文件返回参数
    /// </summary>
    public sealed class CompressFileUploadOutput
    {
        /// <summary>
        /// 文件存储路径
        /// </summary>
        public string FileUrl { get; set; }

        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 文件的Md5
        /// </summary>
        public string FileMd5 { get; set; }

        /// <summary>
        /// 文件的相对路径
        /// </summary>
        public string FolderPath { get; set; }
    }
}
