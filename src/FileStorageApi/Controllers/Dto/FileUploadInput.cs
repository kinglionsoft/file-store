namespace FileStorageApi.Controllers.Dto
{
    public sealed class FileUploadInput
    {
        /// <summary>
        /// 文件的存储组。不指定时，随机存放
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// 文件扩展名。当文件名为空或者文件名中不包含扩展名时，必须指定
        /// </summary>
        public string Extension { get; set; }
    }
}