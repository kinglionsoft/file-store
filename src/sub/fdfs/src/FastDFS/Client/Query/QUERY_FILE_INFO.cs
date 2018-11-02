using System;
using System.Net;

namespace FastDFS.Client
{
    internal class QUERY_FILE_INFO : FDFSRequest<QUERY_FILE_INFO_Args, QUERY_FILE_INFO_Result>
    {
        public QUERY_FILE_INFO(IPEndPoint pEndPoint)
		{
		    EndPoint = pEndPoint;
		}
	}
}