using System;
using System.Text;

namespace FastDFS.Client
{
    internal class QUERY_STORE_WITH_GROUP_ONE : FDFSRequest<QUERY_STORE_WITH_GROUP_ONE_Args, QUERY_STORE_RESPONSE>
    {
		public QUERY_STORE_WITH_GROUP_ONE()
		{
		    ConnectionType = EConnectionType.Tracker;
        }
	}
}