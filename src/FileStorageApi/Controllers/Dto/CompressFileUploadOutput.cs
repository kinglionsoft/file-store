using Newtonsoft.Json;

namespace FileStorageApi.Controllers.Dto
{
    /// <summary>
    /// 压缩包上传文件返回参数
    /// </summary>
    public sealed class CompressFileUploadOutput
    {
        /// <summary>
        /// 解压的临时在本地的临时路径，用于上传
        /// </summary>
        [JsonIgnore]
        public string TempFilePath { get; set; }

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
        /// 文件的相对路径层级结构(注意：当为压缩包的时候在该层级结尾加 ? 比如 123/abc.rar? 
        /// 123/abc.rar? 表示123文件夹下有abc.rar的压缩包，用于区分123文件夹下有abc.rar文件夹)
        /// </summary>
        public string FolderPath { get; set; }
    }
}
