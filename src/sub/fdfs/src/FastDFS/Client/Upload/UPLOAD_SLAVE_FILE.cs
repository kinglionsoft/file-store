using System;
using System.Net;

namespace FastDFS.Client
{
    internal class UPLOAD_SLAVE_FILE : FDFSRequest<UPLOAD_SLAVE_FILE_Args, UPLOAD_SLAVE_FILE_Result>
    {
        public UPLOAD_SLAVE_FILE(IPEndPoint endPoint)
        {
            EndPoint = endPoint;
        }
    }
}