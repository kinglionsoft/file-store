using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace FastDFS.Client
{
    internal class FDFSRequest
    {
        protected byte[] Body { get; set; }

        protected Connection Connection { get; set; }

        protected FDFSHeader Header { get; set; }

        protected int ConnectionType { get; set; }
        protected IPEndPoint EndPoint { get; set; }

        protected FDFSRequest()
        {
        }

        public virtual FDFSRequest GetRequest(params object[] paramList)
        {
            throw new NotImplementedException();
        }

        protected virtual async Task OpenAsync()
        {
            if (this.ConnectionType == 0)
            {
                this.Connection = await ConnectionManager.GetTrackerConnectionAsync();
            }
            else
            {
                this.Connection = await ConnectionManager.GetStorageConnectionAsync(EndPoint);
            }
            await this.Connection.OpenAsync();
        }

        public virtual async Task<byte[]> GetResponseAsync()
        {
            if (this.ConnectionType == 0)
            {
                this.Connection = await ConnectionManager.GetTrackerConnectionAsync();
            }
            else
            {
                this.Connection = await ConnectionManager.GetStorageConnectionAsync(EndPoint);
            }
            try
            {
                await this.Connection.OpenAsync();

                byte[] num = this.Header.ToByte();
                var buffers = new List<ArraySegment<byte>>(2)
                {
                    new ArraySegment<byte>(num),
                    new ArraySegment<byte>(Body)
                };
                await Connection.SendExAsync(buffers);

                byte[] numArray0 = new byte[10];
                if (await Connection.ReceiveExAsync(numArray0) == 0)
                {
                    throw new FDFSException("Init Header Exeption : Cann't Read Stream");
                }

                var length = Util.BufferToLong(numArray0, 0);
                var command = numArray0[8];
                var status = numArray0[9];

                var fDFSHeader = new FDFSHeader(length, command, status);
                if (fDFSHeader.Status != 0)
                {
                    throw new FDFSStatusException(fDFSHeader.Status, $"Get Response Error,Error Code:{fDFSHeader.Status}");
                }
                byte[] numArray = new byte[fDFSHeader.Length];
                if (fDFSHeader.Length != (long) 0)
                {
                    await Connection.ReceiveExAsync(numArray);
                }
                return numArray;
            }
            finally
            {
                this.Connection?.Close();
            }
        }

        protected virtual async Task<byte[]> ReceiveAsync()
        {
            byte[] numArray0 = new byte[10];
            if (await Connection.ReceiveExAsync(numArray0) == 0)
            {
                throw new FDFSException("Init Header Exeption : Cann't Read Stream");
            }

            var length = Util.BufferToLong(numArray0, 0);
            var command = numArray0[8];
            var status = numArray0[9];

            var fDFSHeader = new FDFSHeader(length, command, status);
            if (fDFSHeader.Status != 0)
            {
                throw new FDFSStatusException(fDFSHeader.Status, $"Get Response Error,Error Code:{fDFSHeader.Status}");
            }
            byte[] numArray = new byte[fDFSHeader.Length];
            if (fDFSHeader.Length != (long)0)
            {
                await Connection.ReceiveExAsync(numArray);
            }
            return numArray;
        }

        public byte[] ToByteArray()
        {
            throw new NotImplementedException();
        }
    }
}