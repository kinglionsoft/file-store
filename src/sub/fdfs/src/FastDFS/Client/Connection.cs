using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace FastDFS.Client
{
    internal class Connection
    {
        private DateTime _lastUseTime;

        public bool InUse { get; set; } = false;

        public Pool Pool { get; set; }

        private readonly IPEndPoint _endPoint;

        private Socket _socket { get; set; }

        public Connection(IPEndPoint endPoint)
        {
            _endPoint = endPoint;
        }

        internal void Close()
        {
            this.Pool.CloseConnection(this);
        }

        internal async Task OpenAsync()
        {
            if (this.InUse)
            {
                throw new FDFSException("the connection is already in user");
            }

            if (_lastUseTime != default(DateTime) && IdleTotalSeconds > FDFSConfig.ConnectionLifeTime)
            {
                await CloseSocketAsync();
            }

            this.InUse = true;
            this._lastUseTime = DateTime.Now;

            if (_socket == null || !_socket.Connected)
            {
                _socket = NewSocket();
                await _socket.ConnectExAsync(_endPoint);
            }
        }

        private double IdleTotalSeconds
        {
            get { return (DateTime.Now - _lastUseTime).TotalSeconds; }
        }

        private Socket NewSocket()
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.LingerState = new LingerOption(false, 0);
            socket.NoDelay = true;
            return socket;
        }

        internal async Task<int> SendExAsync(IList<ArraySegment<byte>> buffers)
        {
            try
            {
                return await _socket.SendExAsync(buffers);
            }
            catch
            {
                await CloseSocketAsync();
                throw;
            }
        }

        internal async Task<int> SendExAsync(byte[] buffers)
        {
            try
            {
                return await _socket.SendExAsync(buffers);
            }
            catch
            {
                await CloseSocketAsync();
                throw;
            }
        }

        internal async Task<long> SendExAsync(Stream stream, CancellationToken token)
        {
            try
            {
                return await _socket.SendExAsync(stream, token);
            }
            catch 
            {
                await CloseSocketAsync();
                throw;
            }
        }

        internal async Task<int> ReceiveExAsync(byte[] buffer)
        {
            var sent = await _socket.ReceiveExAsync(buffer);
            if (sent == 0)
            {
                await CloseSocketAsync();
            }
            return sent;
        }

        private async Task CloseSocketAsync()
        {
            try
            {
                byte[] buffer0 = new FDFSHeader(0L, 0x52, 0).ToByte();
                await _socket.SendExAsync(buffer0);
                _socket.Close();
                _socket.Dispose();
            }
            catch
            {
                // ignored
            }
            _socket = null;
        }
    }
}