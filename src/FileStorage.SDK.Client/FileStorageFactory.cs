namespace FileStorage.SDK.Client
{
    public static class FileStorageFactory
    {
        private static IFileStorage _fileStorage;

        public static void Initialize(FileStorageOption option)
        {
            _fileStorage = new FileStorage(option);
        }

        public static IFileStorage Create()
        {
            return _fileStorage;
        }
    }
}