using System;
using System.Collections;
using System.Collections.Generic;

namespace FastDFS.Client
{
    internal class QUERY_STORE_WITH_GROUP_ONE_Args: IRequestArgs
    {
        private readonly string _groupName;

        public QUERY_STORE_WITH_GROUP_ONE_Args(string groupName)
        {
            _groupName = groupName;
        }

        public IEnumerator<object> GetEnumerator()
        {
            if (string.IsNullOrWhiteSpace(_groupName))
            {
                yield return new FDFSHeader(0L,
                        ProtoCommon.TrackerProtoCmdServiceQueryStoreWithoutGroupOne, 0)
                    .ToByte();
            }
            else
            {
                var groupBuffer = Util.StringToByte(_groupName);
                if ((int) groupBuffer.Length > 16)
                {
                    throw new FDFSException("GroupName is too long");
                }

                var body = new byte[16];
                Array.Copy(groupBuffer, 0, body, 0, groupBuffer.Length);
                yield return new FDFSHeader((long) 16,
                    ProtoCommon.TrackerProtoCmdServiceQueryStoreWithGroupOne,
                    0);
                yield return body;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}