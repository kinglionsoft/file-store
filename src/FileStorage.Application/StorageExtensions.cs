using System.Linq;
using System.Net;
using FastDFS.Client;
using FileStorage.Core;
using FileStorage.FDFS;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FileStorage.Application
{
    public static class StorageExtensions
    {
        public static void AddStorage(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<FastDfsOption>(configuration);
            services.AddSingleton<IStorageProvider, FastDfsStorageProvider>();
            services.AddSingleton<IFileStorageService, FileStorageService>();
        }
    }
}