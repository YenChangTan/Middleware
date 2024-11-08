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
using Middleware.Fundamental;

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
            ModeConfiguration selectedConfig = new ModeConfiguration();
            var modeConfigurations = Configuration.GetSection("ModeConfigurations").Get<List<ModeConfiguration>>();
            var selectedModeId = Configuration["SelectedModeId"];
            TCP tcp = new TCP();
            
            foreach (var modeConfiguration in modeConfigurations)
            {
                if (modeConfiguration.ModeId == selectedModeId)
                {
                    selectedConfig = modeConfiguration;
                }
            }

            if (selectedConfig.EndpointType == "TCP")
            {
                services.AddSingleton(tcp);
                services.AddHostedService<TCPClientService>();
            }
            else if (selectedConfig.EndpointType == "OPC")
            {
                services.AddHostedService<OPCClientService>();
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

            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = null;
                });
        }
    }
}
