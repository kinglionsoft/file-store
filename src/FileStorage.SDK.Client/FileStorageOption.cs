namespace FileStorage.SDK.Client
{
    public sealed class FileStorageOption
    {
        /// <summary>
        /// 文件服务器地址
        /// </summary>
        public string FileServer { get; set; }

        /// <summary>
        /// 获取访问Token的地址
        /// </summary>
        public string IdentityServer { get; set; }

        internal const string UploadUrl = "/file/upload";

        internal const string DeleteUrl = "/file/delete";

        internal const string DownloadUrl = "/file/download";
    }
}