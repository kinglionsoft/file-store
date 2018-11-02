using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace FastDFS.Client
{
    internal abstract class FDFSRequest<TArgs, TResult>
        where TArgs : IRequestArgs
        where TResult : IRequestResult, new()
    {
        protected byte[] Body { get; set; }

        protected Connection Connection { get; set; }

        protected FDFSHeader Header { get; set; }

        protected EConnectionType ConnectionType { get; set; } = EConnectionType.Storage;

        protected IPEndPoint EndPoint { get; set; }
        
        public virtual async Task<TResult> RequestAsync(TArgs args, CancellationToken token)
        {
            try
            {
                if (this.ConnectionType == EConnectionType.Tracker)
                {
                    this.Connection = await ConnectionManager.GetTrackerConnectionAsync();
                }
                else
                {
                    this.Connection = await ConnectionManager.GetStorageConnectionAsync(EndPoint);
                }
                await this.Connection.OpenAsync();
                var tasks = args
                    .Select<object, Task>(x =>
                    {
                        if (x is byte[] buffer)
                        {
                            return Connection.SendExAsync(buffer);
                        }

                        if (x is Stream stream)
                        {
                            return Connection.SendExAsync(stream, token);
                        }
                        throw new NotSupportedException();
                    })
                    .ToArray();
                await Task.WhenAll(tasks);

                var responseBuffer = await ReceiveAsync();
                var result = new TResult();
                result.Deserialize(responseBuffer);
                return result;
            }
            finally
            {
                Connection?.Close();
            }
        }

        protected virtual async Task OpenAsync()
        {
            if (this.ConnectionType == EConnectionType.Tracker)
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
            try
            {
                await this.OpenAsync();

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
                if (fDFSHeader.Length != (long)0)
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
    }


}