using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace FastDFS.Client
{
    internal class UPLOAD_SLAVE_FILE_Args : IRequestArgs
    {
        private readonly string _masterFileName;
        private readonly string _prefix;
        private readonly string _fileExt;
        private readonly Stream _stream;

        public UPLOAD_SLAVE_FILE_Args(string masterFileName, string prefix, string fileExt, Stream stream)
        {
            _masterFileName = masterFileName;
            _prefix = prefix;
            _fileExt = fileExt;
            _stream = stream;
        }

        public IEnumerator<object> GetEnumerator()
        {
            byte[] masterFileBuffer = Util.StringToByte(_masterFileName); 

            long bodyLength = 2 * ProtoCommon.FdfsProtoPkgLenSize
                              + ProtoCommon.FdfsFilePrefixMaxLen 
                              + ProtoCommon.FdfsFileExtNameMaxLen
                              + masterFileBuffer.Length
                              + _stream.Length;

            var header = new FDFSHeader(bodyLength, ProtoCommon.StorageProtoCmdUploadSlaveFile, 0).ToByte();

            var package = new byte[header.Length + bodyLength - _stream.Length];

            Array.Copy(header, 0, package, 0, header.Length);
            long offset = header.Length;

            var masterFileLengthBuffer = Util.LongToBuffer(_masterFileName.Length);
            Array.Copy(masterFileLengthBuffer, 0, package, offset, masterFileLengthBuffer.Length);
            offset += masterFileLengthBuffer.Length;

            var indexBuffer = Util.LongToBuffer(_stream.Length);
            Array.Copy(indexBuffer, 0, package, offset, indexBuffer.Length);
            offset += indexBuffer.Length;

            //byte[] indexBuffer = Util.LongToBuffer(_storageNode.StorePathIndex);
            //Array.Copy(indexBuffer, 0, body, offset, indexBuffer.Length);
            //offset += indexBuffer.Length;

            byte[] prefixBuffer = Util.StringToByte(_prefix);
            Array.Copy(prefixBuffer, 0, package, offset, prefixBuffer.Length);
            offset += ProtoCommon.FdfsFilePrefixMaxLen;

            var extBuffer = Util.StringToByte(_fileExt);
            Array.Copy(extBuffer, 0, package, offset, extBuffer.Length);
            offset += ProtoCommon.FdfsFileExtNameMaxLen;

            Array.Copy(masterFileBuffer, 0, package, offset, masterFileBuffer.Length);
           
            yield return package;
            yield return _stream;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}