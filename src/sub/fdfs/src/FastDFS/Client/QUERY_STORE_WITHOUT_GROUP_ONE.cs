using System;

namespace FastDFS.Client
{
    internal class QUERY_STORE_WITHOUT_GROUP_ONE: FDFSRequest<UploadArgs, UploadResult>
    {
        private static QUERY_STORE_WITHOUT_GROUP_ONE _instance;

        public static QUERY_STORE_WITHOUT_GROUP_ONE Instance
            => _instance ?? (_instance = new QUERY_STORE_WITHOUT_GROUP_ONE());

        private QUERY_STORE_WITHOUT_GROUP_ONE()
        {
            ConnectionType = EConnectionType.Tracker;
        }

        public FDFSRequest<UploadArgs, UploadResult> GetRequest(params object[] paramList)
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