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
        private readonly string selectedModeId;
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            selectedModeId = configuration.GetValue<string>("SelectedModeId");
        }

        public void ConfigureServices(IServiceCollection services)
        {
            ModeConfiguration selectedConfig = new ModeConfiguration();
            var modeConfigurations = new List<ModeConfiguration>();
            Configuration.GetSection("ModeConfigurations").Bind(modeConfigurations);
            //var modeConfigurations = Configuration.GetSection("ModeConfigurations").Get<List<ModeConfiguration>>();
            //var selectedModeId = Configuration.GetValue<string>("SelectedModeId");
            foreach (var modeConfiguration in modeConfigurations)
            {
                if (modeConfiguration.ModeId == selectedModeId)
                {
                    selectedConfig = modeConfiguration;
                    services.AddSingleton<ModeConfiguration>(selectedConfig);
                    
                }
            }
            if (selectedConfig.EndpointType == "TCP")
            {
                TCP tcp = new TCP();
                services.AddSingleton<TCP>(tcp);
                services.AddSingleton<TaskSyncService>();
                services.AddHostedService<TCPServerService>();
                
            }
            BLLServer.SetBaseAddress(selectedConfig.MESIP, selectedConfig.MESPort);
            BLLServer.SetBearerToken(selectedConfig.MESToken);
            BLLServer.SetTimeOut(Configuration.GetValue<int>("RequestTimeOut"));
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = null;
                });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage(); // Use developer exception page
            }

            app.UseRouting(); // Use routing

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers(); // Map controller endpoints
            });
            //await Task.Run(() => opcClient.LoadConfig());
        }
    }
}
