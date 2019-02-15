using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FileStorage.Core;
using FileStorage.SDK.Client;
using Newtonsoft.Json;
using Xunit;

namespace Client.Test
{
    public class StorageTest
    {
        public StorageTest()
        {
            FileStorageFactory.Initialize(new FileStorageOption
            {
                FileServer = "http://localhost:5000"
            });
        }

        [Fact]
        public async Task UploadAndDelete()
        {
            var storage = FileStorageFactory.Create();
            foreach (var file in GetFiles())
            {
                var result = await storage.UploadAsync(file);
                Assert.True(result.Success);
                Assert.Equal(1, result.Data.Length);
                var delete = await storage.TryDeleteAsync(result.Data[0]);
                Assert.True(delete);
            }
        }

        [Fact]
        public async Task Download()
        {
            const string url = "http://store.yitu666.com:8880/group1/M00/00/68/oYYBAFxmcp6AQsyIAAAUa4KwmUU490.png";

            using (var httpClient = new HttpClient())
            {
                var content = JsonConvert.SerializeObject(new FilesDownloadModel
                {
                    FileName = "test.zip",
                    Files = new Dictionary<string, string>
                    {
                        {"1.png", url},
                        {"img/2.png", url}
                    }
                });
                var response = await httpClient.PostAsync("http://localhost:5000/File/Download",
                    new StringContent(content, Encoding.UTF8, "application/json"));

                Assert.True(response.IsSuccessStatusCode);

                using (var zip = new ZipArchive(await response.Content.ReadAsStreamAsync()))
                {
                    var files = zip.Entries.Select(x => x.FullName).ToList();
                    Assert.Contains("1.png", files);
                    Assert.Contains("img/2.png", files);
                }
            }
        }

        string[] GetFiles()
        {
            return Directory.GetFiles("files");
        }
    }
}
