using System.Net;

namespace FastDFS.Client
{
    internal class UPLOAD_FILE : FDFSRequest<UploadArgs, UploadResult>
    {
        public UPLOAD_FILE(IPEndPoint endPoint)
        {
            EndPoint = endPoint;
        }
    }
}