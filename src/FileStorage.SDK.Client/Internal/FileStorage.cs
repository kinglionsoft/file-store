using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
//using FileStorage.Core;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace FileStorage.SDK.Client
{
    internal class FileStorage : IFileStorage
    {
        private readonly FileStorageOption _option;
        private readonly HttpClient _httpClient;

        public FileStorage(IOptions<FileStorageOption> optionAccessor)
            : this(optionAccessor.Value)
        {
        }

        internal FileStorage(FileStorageOption option)
        {
            _option = option;

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_option.FileServer)
            };

            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<ApiResult<string[]>> UploadAsync(IEnumerable<string> files,
            string defaultExtension = null,
            string groupName = null,
            CancellationToken token = default)
        {
            using (var postContent = new MultipartFormDataContent())
            {
                if (!string.IsNullOrWhiteSpace(defaultExtension))
                {
                    postContent.Add(new StringContent(defaultExtension), "Extension");
                }

                if (!string.IsNullOrWhiteSpace(groupName))
                {
                    postContent.Add(new StringContent(groupName), "Group");
                }

                foreach (var file in files)
                {
                    var fi = new FileInfo(file);
                    var streamContent = new StreamContent(fi.OpenRead());
                    postContent.Add(streamContent, "file", fi.Name);
                }

                HttpResponseMessage response = null;
                try
                {
                    response = await _httpClient.PostAsync(FileStorageOption.UploadUrl,
                        postContent,
                        token);

                    var body = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<ApiResult<string[]>>(body);
                    return result;
                }
                catch (Exception ex)
                {
                    if (ex is HttpRequestException)
                    {
                        throw new Exception("上传文件失败", ex);
                    }

                    if (response != null && !response.IsSuccessStatusCode)
                    {
                        throw new Exception($"上传文件失败，状态码：{(int)response.StatusCode}", ex);
                    }

                    throw new Exception("上传文件失败", ex);
                }
                finally
                {
                    response?.Dispose();
                }
            }
        }

        public async Task<ApiResult> DeleteAsync(string fileUrl,
            CancellationToken token = default)
        {
            if (string.IsNullOrWhiteSpace(fileUrl))
            {
                throw new ArgumentNullException();
            }
            HttpResponseMessage response = null;
            try
            {
                HttpContent content = new FormUrlEncodedContent(
                    new[]
                    {
                        new KeyValuePair<string, string>("url", fileUrl)
                    });
                response = await _httpClient.PostAsync(FileStorageOption.DeleteUrl,
                    content,
                    token);
                var body = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiResult>(body);
                return result;
            }
            catch (Exception ex)
            {
                if (ex is HttpRequestException)
                {
                    throw new Exception("删除文件失败", ex);
                }

                if (response != null && !response.IsSuccessStatusCode)
                {
                    throw new Exception($"删除文件失败，状态码：{(int)response.StatusCode}", ex);
                }

                throw new Exception("删除文件失败", ex);
            }
            finally
            {
                response?.Dispose();
            }
        }

        public async Task<Stream> DownloadAsync(FilesDownloadModel input, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(input.FileName) || !(input.Files?.Count > 0))
            {
                throw new ArgumentException("参数无效");
            }

            var response = await _httpClient.PostAsync(FileStorageOption.DownloadUrl,
                new StringContent(JsonConvert.SerializeObject(input),
                    Encoding.UTF8, 
                    "application/json"),
                token);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStreamAsync();
            }
            throw new HttpRequestException($"下载文件失败，{response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
        }
    }
}