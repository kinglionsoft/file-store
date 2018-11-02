namespace FastDFS.Client
{
    internal interface IRequestResult
    {
        void Deserialize(byte[] buffer);
    }
}