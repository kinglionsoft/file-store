namespace FastDFS.Client
{
    internal class DELETE_FILE_Result: IRequestResult
    {
        public readonly bool Success = true;
        public void Deserialize(byte[] buffer)
        {
        }
    }
}