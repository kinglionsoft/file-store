using System.Net;

namespace FastDFS.Client
{
    internal class UPLOAD_FILE : FDFSRequest<UPLOAD_FILE_Args, UPLOAD_FILE_Result>
    {
        public UPLOAD_FILE(IPEndPoint endPoint)
        {
            EndPoint = endPoint;
        }
    }
}