using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;

namespace FastDFS.Client
{
    internal class QUERY_FILE_INFO_Args: IRequestArgs
    {
        private readonly StorageNode _storageNode;
        private readonly string _fileName;

        public QUERY_FILE_INFO_Args(StorageNode storageNode, string fileName)
        {
            _storageNode = storageNode;
            _fileName = fileName;
        }

        public IEnumerator<object> GetEnumerator()
        {
            long length = 16 + _fileName.Length;
            byte[] numArray = new byte[length];
            byte[] num = Util.StringToByte(_storageNode.GroupName);
            byte[] num1 = Util.StringToByte(_fileName);
            Array.Copy(num, 0, numArray, 0, (int)num.Length);
            Array.Copy(num1, 0, numArray, 16, (int)num1.Length);
            yield return new FDFSHeader(length, 22, 0).ToByte();
            yield return numArray;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}