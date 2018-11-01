using System;

namespace FastDFS.Client
{
    internal class QUERY_STORE_WITHOUT_GROUP_ONE: FDFSRequest
    {
        private static QUERY_STORE_WITHOUT_GROUP_ONE _instance;

        public static QUERY_STORE_WITHOUT_GROUP_ONE Instance
            => _instance ?? (_instance = new QUERY_STORE_WITHOUT_GROUP_ONE());

        private QUERY_STORE_WITHOUT_GROUP_ONE() { }

        public override FDFSRequest GetRequest(params object[] paramList)
        {
            var queryStoreWithoutGroupOne = new QUERY_STORE_WITHOUT_GROUP_ONE
            {
                Header = new FDFSHeader(0L,
                    ProtoCommon.TrackerProtoCmdServiceQueryStoreWithoutGroupOne, 0),
                Body = new byte[0]
            };
            return queryStoreWithoutGroupOne;
        }
    }
}