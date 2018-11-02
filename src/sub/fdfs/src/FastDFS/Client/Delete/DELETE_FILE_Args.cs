using System;
using System.Collections;
using System.Collections.Generic;

namespace FastDFS.Client
{
    internal class DELETE_FILE_Args: IRequestArgs
    {
        public readonly string _groupName;
        public readonly string _fileName;

        public DELETE_FILE_Args(string groupName, string fileName)
        {
            _groupName = groupName;
            _fileName = fileName;
        }

        public IEnumerator<object> GetEnumerator()
        {
            long length = ProtoCommon.FdfsGroupNameMaxLen  + _fileName.Length;
            var header = new FDFSHeader(length, 12, 0).ToByte();
            var package = new byte[header.Length + length];

            Array.Copy(header, 0, package, 0, header.Length);
            long offset = header.Length;

            var groupNameBuffer = Util.StringToByte(_groupName);
            Array.Copy(groupNameBuffer, 0, package, offset,  groupNameBuffer.Length);
            offset += ProtoCommon.FdfsGroupNameMaxLen;


            var fileNameBuffer = Util.StringToByte(_fileName);
            Array.Copy(fileNameBuffer, 0, package, offset, fileNameBuffer.Length);

            yield return package;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}