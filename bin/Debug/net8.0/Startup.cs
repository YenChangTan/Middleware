using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Middleware.BackgroundTask;
using Middleware.Controller;
using Middleware.Model;
using Middleware.Model.AMR;
using Middleware.Model.OPC;
using Middleware.DataHolder;
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
                    services.AddSingleton<TCP>();
                    services.AddSingleton<OPCUA>();

                }
            }
            services.AddSingleton<TaskSyncService>();
            if (selectedConfig.EndpointType == "TCP")
            {
                if (selectedConfig.RobotName == "Robco 2")
                {
                    BLLServer.setDeviceAddress(selectedConfig.DeviceIp, selectedConfig.DevicePort, "api/task");
                }
                services.AddHostedService<TCPServerService>();

            }
            else if (selectedConfig.EndpointType == "TCPClient")
            {
                services.AddHostedService<TCPClientService>();
            }
            else if (selectedConfig.EndpointType == "API")
            {
                AMRTaskMapping.amrTaskMapping = JsonSerializer.Deserialize<Dictionary<string, TaskMapping>>(File.ReadAllText("AGVTaskList.json"));
                BLLServer.setDeviceAddress(selectedConfig.DeviceIp, selectedConfig.DevicePort, "api/fexa/");
                services.AddHostedService<AMRServerService>();
            }
            else if (selectedConfig.EndpointType == "Magazine")
            {
                services.AddHostedService<MagazineLoaderService>();
            }
            else if (selectedConfig.EndpointType == "OPC")
            {
                foreach (var opc in selectedConfig.OPC)
                {
                    OPCConnections.opcConnections.Add(new OPCConnection()
                    {
                        OPCName = opc.OPCName,
                        Recipe = opc.Recipe,
                        NodeBase = opc.NodeBase
                    });
                }
                BLLServerForOPC.SetBaseAddress(selectedConfig.MESIP, selectedConfig.MESPort);
                services.AddHostedService<OPCClientService>();
            }
            else
            {
                services.AddHostedService<AMRServerService>();
            }
            BLLServer.SetBaseAddress(selectedConfig.MESIP, selectedConfig.MESPort);
            BLLServer.SetBearerToken(selectedConfig.MESToken);
            BLLServer.SetMESTimeOut(selectedConfig.MESRequestTimeOut);
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
