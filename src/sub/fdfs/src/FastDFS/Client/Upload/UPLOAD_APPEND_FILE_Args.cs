using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace FastDFS.Client
{
    internal class UPLOAD_APPEND_FILE_Args: IRequestArgs
    {
        private readonly StorageNode _storageNode;
        private readonly byte[] _fileBuffer;
        private readonly string _fileExt;

        public UPLOAD_APPEND_FILE_Args(StorageNode storageNode,
            byte[] fileBuffer,
            string fileExt)
        {
            _storageNode = storageNode;
            _fileBuffer = fileBuffer;
            _fileExt = fileExt;
        }

        public IEnumerator<object> GetEnumerator()
        {
            var extBuffer = new byte[6];
            var ext = Util.StringToByte(_fileExt);
            Array.Copy(ext, 0, extBuffer, 0, ext.Length);
           
            long length1 = 15 + _fileBuffer.Length;
            var body = new byte[length1];
            body[0] = _storageNode.StorePathIndex;
            var lengthBuffer = Util.LongToBuffer((long)_fileBuffer.Length);
            Array.Copy(lengthBuffer, 0, body, 1, (int)lengthBuffer.Length);
            Array.Copy(extBuffer, 0, body, 9, (int)extBuffer.Length);
            Array.Copy(_fileBuffer, 0, body, 15, (int)_fileBuffer.Length);

            yield return new FDFSHeader(length1, 23, 0).ToByte();
            yield return body;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}