using System;
using System.Net;

namespace FastDFS.Client
{
    internal class UPLOAD_APPEND_FILE : FDFSRequest<UPLOAD_APPEND_FILE_Args, UPLOAD_APPEND_FILE_Result>
    {
        public UPLOAD_APPEND_FILE(IPEndPoint pEndPoint)
		{
		    EndPoint = pEndPoint;
		}
	}
}