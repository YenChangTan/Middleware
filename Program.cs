using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace Middleware
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    var basePath = Directory.GetCurrentDirectory();
                    config.SetBasePath(basePath)
                        .AddJsonFile("appsetting.json", optional: false, reloadOnChange: true);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    var basePath = Directory.GetCurrentDirectory();
                    var configuration = new ConfigurationBuilder()
                        .SetBasePath(basePath)
                        .AddJsonFile("appsetting.json")
                        .Build();
                    var applicationUrl = configuration["ApplicationUrl"];
                    webBuilder.UseStartup<Startup>()
                        .UseUrls(applicationUrl);

                });
    }
}
