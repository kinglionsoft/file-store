using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace FastDFS.Client
{
    internal class UploadArgs : IRequestArgs
    {
        private readonly StorageNode _storageNode;
        private readonly Stream _fileStream;
        private readonly string _fileExt;

        public UploadArgs(StorageNode storageNode,
            Stream fileStream,
            string fileExt)
        {
            _storageNode = storageNode;
            _fileStream = fileStream;
            _fileExt = fileExt;
        }

        public IEnumerator<object> GetEnumerator()
        {

            /*
                * 组装协议Header. header共10字节，格式如下：
                     8 bytes body length
                     1 byte command
                     1 byte status
                */
            var header = new FDFSHeader(15 + _fileStream.Length, ProtoCommon.StorageProtoCmdUploadFile, 0);
            /*
             * 组装Body
             *  0       StorePathIndex
             *  1-8     文件长度
             *  9-14    扩展名
             *  15 +    文件
             */

            var extBuffer = Util.StringToByte(_fileExt);
            var fileHeader = new byte[15]; // 文件头
            fileHeader[0] = _storageNode.StorePathIndex;

            var lengthBuffer = Util.LongToBuffer(_fileStream.Length);
            Array.Copy(lengthBuffer, 0, fileHeader, 1, lengthBuffer.Length);
            Array.Copy(extBuffer, 0, fileHeader, 9, extBuffer.Length);

            yield return header.ToByte();
            yield return fileHeader;
            yield return _fileStream;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}