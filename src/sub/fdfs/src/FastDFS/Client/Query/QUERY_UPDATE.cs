using System;
using System.Text;

namespace FastDFS.Client
{
    internal class QUERY_UPDATE : FDFSRequest<QUERY_UPDATE_Args, QUERY_UPDATE_Result>
    {
		public QUERY_UPDATE()
		{
		    ConnectionType = EConnectionType.Tracker;
		}
	}
}