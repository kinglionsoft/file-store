using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FileStorage.FDFS;
using Microsoft.Extensions.Options;

namespace FileStorage.Application
{
    public interface IFastDfsHttpClient
    {
        Task<Stream> DownloadAsync(string url, CancellationToken cancellationToken);
    }

    internal class FastDfsHttpClient : IFastDfsHttpClient
    {
        private readonly HttpClient _client;

        public FastDfsHttpClient(HttpClient client, IOptions<FastDfsOption> options)
        {
            _client = client;
            _client.BaseAddress = new Uri($"http://{options.Value.TrackerIps[0]}");
        }

        public Task<Stream> DownloadAsync(string url, CancellationToken cancellationToken)
        {
            var uri = new Uri(url);
            var fileUrl = uri.IsAbsoluteUri ? uri.PathAndQuery : url;

            cancellationToken.Register(() => { _client.CancelPendingRequests(); });
            return _client.GetStreamAsync(fileUrl);
        }
    }
}