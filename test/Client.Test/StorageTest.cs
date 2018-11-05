using System;
using System.IO;
using System.Threading.Tasks;
using FileStorage.SDK.Client;
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

        string[] GetFiles()
        {
            return Directory.GetFiles("files");
        }
    }
}
