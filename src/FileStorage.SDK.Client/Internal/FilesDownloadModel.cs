using System.Collections.Generic;

namespace FileStorage.SDK.Client
{
    public sealed class FilesDownloadModel
    {
        /// <summary>
        /// 下载时的文件名称
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 文件名及其URL
        /// </summary>
        public Dictionary<string, string> Files { get; set; }
    }
}