using System;
using FileStorage.SDK.Client;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjectionExtension
    {
        public static void AddFileStorage(this IServiceCollection services, Action<FileStorageOption> option)
        {
            services.Configure<FileStorageOption>(option);
            services.AddSingleton<IFileStorage, FileStorage.SDK.Client.FileStorage>();
        }
    }
}