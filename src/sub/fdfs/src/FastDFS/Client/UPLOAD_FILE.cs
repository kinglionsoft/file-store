using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace FastDFS.Client
{
    internal class UPLOAD_FILE : FDFSRequest
    {
        public UPLOAD_FILE()
        {
            ConnectionType = 1;
        }

        public UPLOAD_FILE(IPEndPoint endPoint)
        {
            EndPoint = endPoint;
            ConnectionType = 1;
        }

        public UPLOAD_FILE(StorageNode node)
        {
            EndPoint = node.EndPoint;
            StorePathIndex = node.StorePathIndex;
            ConnectionType = 1;
        }

        public byte StorePathIndex { get; private set; }

        public override FDFSRequest GetRequest(params object[] paramList)
        {
            if (paramList.Length != 5)
            {
                throw new FDFSException("param count is wrong");
            }
            IPEndPoint endPoint = (IPEndPoint)paramList[0];
            byte num = (byte)paramList[1];
            int num2 = (int)paramList[2];
            string input = (string)paramList[3];
            byte[] sourceArray = (byte[])paramList[4];
            byte[] destinationArray = new byte[6];
            byte[] buffer3 = Util.StringToByte(input);
            int length = buffer3.Length;
            if (length > 6)
            {
                length = 6;
            }
            Array.Copy(buffer3, 0, destinationArray, 0, length);
            UPLOAD_FILE uploadFile = new UPLOAD_FILE
            {
                ConnectionType = 1,
                EndPoint = endPoint
            };
            if (input.Length > 6)
            {
                throw new FDFSException("file ext is too long");
            }
            long num4 = 15 + sourceArray.Length;
            byte[] buffer4 = new byte[num4];
            buffer4[0] = num;
            byte[] buffer5 = Util.LongToBuffer((long)num2);
            Array.Copy(buffer5, 0, buffer4, 1, buffer5.Length);
            Array.Copy(destinationArray, 0, buffer4, 9, destinationArray.Length);
            Array.Copy(sourceArray, 0, buffer4, 15, sourceArray.Length);
            uploadFile.Body = buffer4;
            uploadFile.Header = new FDFSHeader(num4, 11, 0);
            return uploadFile;
        }

        public async Task<Response> SendAsync(Stream fileStream,
            string fileExt,
            CancellationToken token)
        {
            try
            {
                /*
                 * 组装协议Header. header共10字节，格式如下：
                      8 bytes body length
                      1 byte command
                      1 byte status
                 */
                var header = new FDFSHeader(15 + fileStream.Length, ProtoCommon.StorageProtoCmdUploadFile, 0);
                /*
                 * 组装Body
                 *  0       StorePathIndex
                 *  1-8     文件长度
                 *  9-14    扩展名
                 *  15 +    文件
                 */

                var extBuffer = Util.StringToByte(fileExt);
                var fileHeader = new byte[15]; // 文件头
                fileHeader[0] = StorePathIndex;

                var lengthBuffer = Util.LongToBuffer(fileStream.Length);
                Array.Copy(lengthBuffer, 0, fileHeader, 1, lengthBuffer.Length);
                Array.Copy(extBuffer,0, fileHeader, 9, extBuffer.Length);

                // 发送Header
                await OpenAsync();
                await Connection.SendExAsync(new List<ArraySegment<byte>>(2)
                {
                    new ArraySegment<byte>(header.ToByte()),
                    new ArraySegment<byte>(fileHeader)
                });
                // 发送文件
                await Connection.SendExAsync(fileStream, token);
                // 接收
                var responseBody = await ReceiveAsync();

                return new Response(responseBody);
            }
            finally
            {
                Connection?.Close();
            }
        }

        // Nested Types
        public class Response
        {
            // Fields
            public string FileName;
            public string GroupName;

            // Methods
            public Response(byte[] responseBody)
            {
                byte[] destinationArray = new byte[0x10];
                Array.Copy(responseBody, destinationArray, 0x10);
                this.GroupName = Util.ByteToString(destinationArray).TrimEnd(new char[1]);
                byte[] buffer2 = new byte[responseBody.Length - 0x10];
                Array.Copy(responseBody, 0x10, buffer2, 0, buffer2.Length);
                this.FileName = Util.ByteToString(buffer2).TrimEnd(new char[1]);
            }
        }
    }
}