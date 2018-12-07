namespace FileStorageApi
{
    public sealed class UploadOption
    {
        public int MaxUpload { get; set; } = 10;

        /// <summary>
        /// 单次上传的文件大小总和上限
        /// </summary>
        public int MaxRequestSize { get; set; } = 400;
    }
}