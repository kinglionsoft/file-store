using System;
using System.Text;

namespace FastDFS.Client
{
    internal class QUERY_STORE_WITH_GROUP_ONE : FDFSRequest
	{
		private static QUERY_STORE_WITH_GROUP_ONE _instance;

		public static QUERY_STORE_WITH_GROUP_ONE Instance
		{
			get
			{
				if (QUERY_STORE_WITH_GROUP_ONE._instance == null)
				{
					QUERY_STORE_WITH_GROUP_ONE._instance = new QUERY_STORE_WITH_GROUP_ONE();
				}
				return QUERY_STORE_WITH_GROUP_ONE._instance;
			}
		}

		static QUERY_STORE_WITH_GROUP_ONE()
		{
			QUERY_STORE_WITH_GROUP_ONE._instance = null;
		}

		private QUERY_STORE_WITH_GROUP_ONE()
		{
		}

		public override FDFSRequest GetRequest(params object[] paramList)
		{
			if ((int)paramList.Length == 0)
			{
				throw new FDFSException("GroupName is null");
			}
			QUERY_STORE_WITH_GROUP_ONE queryStoreWithGroupOne = new QUERY_STORE_WITH_GROUP_ONE();
			byte[] num = Util.StringToByte((string)paramList[0]);
			if ((int)num.Length > 16)
			{
				throw new FDFSException("GroupName is too long");
			}
			byte[] numArray = new byte[16];
			Array.Copy(num, 0, numArray, 0, (int)num.Length);
			queryStoreWithGroupOne.Body = numArray;
			queryStoreWithGroupOne.Header = new FDFSHeader((long)16, 104, 0);
			return queryStoreWithGroupOne;
		}
	}
}