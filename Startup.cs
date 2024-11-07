using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Middleware.BackgroundTask;
using Middleware.Controller;
using Middleware.Model;

namespace Middleware
{
    public class Startup
    {
        public IConfiguration Configuration;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<List<ModeConfiguration>>(Configuration.GetSection("ModeConfigurations"));
            var selectedModeId = Configuration["SelectedModeId"];
            var selectedConfig = Configuration.GetSection("ModeConfigurations")
                .Get<List<ModeConfiguration>>()
                .FirstOrDefault(config => config.ModeId == selectedModeId);
            if (selectedConfig.EndpointType == "TCP")
            {
                services.AddHostedService<TCPClientService>();
                services.AddHostedService<TCPServerService>();
            }
            else if (selectedConfig.EndpointType == "OPC")
            {
            }
            else if (selectedConfig.EndpointType == "NA")
            {

            }
            services.AddSingleton(provider =>
                {
                    var modeConfiguration = provider.GetRequiredService<IOptions<List<ModeConfiguration>>>().Value;
                    var selectedConfig = modeConfiguration.FirstOrDefault(config => config.ModeId == selectedModeId);
                    if (selectedConfig == null)
                    {
                        throw new InvalidOperationException($"Configuration for ModeId '{selectedModeId}' not found.");
                    }

                    return selectedConfig;

                }
            );

            services.AddHostedService<TCPClientService>();

            services.AddHostedService(provider =>
            {
                var selectedConfig = provider.GetRequiredService<ModeConfiguration>();

                if (selectedConfig.EndpointType == "TCP")
                {
                    return new TCPClientService(); // Register TcpClientService
                }
                else if (selectedConfig.EndpointType == "OPC")
                {
                    return new OPCClientService(); // Register OpcClientService
                }
                else if (selectedConfig.EndpointType == "NA")
                {
                    return new TCPServerService(); // Register TaskSchedulerService for NA
                }

                throw new InvalidOperationException("No valid background service found for the selected configuration.");
            });

            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = null;
                });
        }
    }
}
