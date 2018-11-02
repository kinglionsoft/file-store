using System;
using System.Net;

namespace FastDFS.Client
{
    internal class DELETE_FILE : FDFSRequest<DELETE_FILE_Args, DELETE_FILE_Result>
    {
        public DELETE_FILE(IPEndPoint endPoint)
        {
            EndPoint = endPoint;
        }
    }
}