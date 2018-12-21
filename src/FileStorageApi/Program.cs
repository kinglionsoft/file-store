using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FileStorageApi
{
    internal static class Program
    {
        /// <summary>
        /// 这是服务主机进程的入口点。
        /// </summary>
        private static void Main()
        {
            DebugHost();
            return;

            try
            {
                // ServiceManifest.XML 文件定义一个或多个服务类型名称。
                // 注册服务会将服务类型名称映射到 .NET 类型。
                // 在 Service Fabric 创建此服务类型的实例时，
                // 会在此主机进程中创建类的实例。

                ServiceRuntime.RegisterServiceAsync("FileStorageApiType",
                    context => new FileStorageApi(context)).GetAwaiter().GetResult();

                ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id, typeof(FileStorageApi).Name);

                // 防止此主机进程终止，以使服务保持运行。 
                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.ServiceHostInitializationFailed(e.ToString());
                throw;
            }
        }

        private static void DebugHost()
        {
            new WebHostBuilder()
                    .UseKestrel((hostingContext, options) =>
                    {
                        var maxBodySize = hostingContext.Configuration["Upload:MaxRequestSize"];
                        if (!string.IsNullOrWhiteSpace(maxBodySize)
                            && int.TryParse(maxBodySize, out var mrbs))
                        {
                            options.Limits.MaxRequestBodySize = mrbs * 1024L * 1024L;
                        }
                    })
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .ConfigureAppConfiguration((hostingContext, config) =>
                    {
                        config
                            .SetBasePath(hostingContext.HostingEnvironment.ContentRootPath)
                            .AddJsonFile(Path.Combine("config", "appsettings.json"), false, true)
                            .AddJsonFile(Path.Combine("config", $"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json"), true, true)
                            .AddJsonFile(Path.Combine("configMap", $"appsettings.k8s.json"), true, true)
                            .AddEnvironmentVariables();
                    })
                    .ConfigureLogging((hostingContext, logging) =>
                    {
                        //add logging
                        logging.AddConsole();
                        if (hostingContext.HostingEnvironment.IsDevelopment())
                        {
                            logging.SetMinimumLevel(LogLevel.Debug);
                        }
                        else
                        {
                            logging.SetMinimumLevel(LogLevel.Error);
                        }
                    })
                    .UseIISIntegration()
                    .UseStartup<Startup>()
                    .Build()
                    .Run();
        }
    }
}
