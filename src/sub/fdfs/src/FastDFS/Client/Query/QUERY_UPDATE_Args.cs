using System;
using System.Collections;
using System.Collections.Generic;

namespace FastDFS.Client
{
    internal class QUERY_UPDATE_Args: IRequestArgs
    {
        private readonly string _groupName;
        private readonly string _fileName;

        public QUERY_UPDATE_Args(string groupName, string fileName)
        {
            if (groupName.Length > 16)
            {
                throw new FDFSException("GroupName is too long");
            }
            _groupName = groupName;
            _fileName = fileName;
        }


        public IEnumerator<object> GetEnumerator()
        {
            byte[] @group = Util.StringToByte(_groupName);
            byte[] file = Util.StringToByte(_fileName);
            int length = 16 + file.Length;
            byte[] body = new byte[length];
            Array.Copy(@group, 0, body, 0, @group.Length);
            Array.Copy(file, 0, body, 16, file.Length);
            yield return new FDFSHeader(length, 103, 0).ToByte();
            yield return body;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}