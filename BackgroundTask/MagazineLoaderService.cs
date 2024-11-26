using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.Hosting;
using Middleware.Controller;
using Middleware.DataHolder;
using Middleware.Fundamental;
using Middleware.Model;
using Newtonsoft.Json;

namespace Middleware.BackgroundTask
{
    public class MagazineLoaderService : BackgroundService
    {
        public ModeConfiguration _modeConfiguration = new ModeConfiguration();
        public TCP _tcp = new TCP();
        public MagazineLoaderService(ModeConfiguration modeConfiguration, TCP tcp)
        {
            _modeConfiguration = modeConfiguration;
            _tcp = tcp;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            int resultCode = 0;
            bool MagazineCounter = false;
            BLLServer server = new BLLServer();
            while (true)
            {
                try
                {
                    while (!_tcp.ConnectTcp(_modeConfiguration.Server.First().IP, _modeConfiguration.Server.First().Port.ToString()))
                    {
                        
                        Logger.LogMessage("Fail to connect, reconnecting", "error");
                        await Task.Delay(1000);
                    }

                    while (true)
                    {
                        resultCode = 0;
                        resultCode = await _tcp.ReceiveFromMagazineLoader();
                        
                        if (resultCode == 1)
                        {
                            Logger.LogMessage("11", "TCP");
                            DateTime Now = DateTime.Now;
                            LoaderReportData.loaderReport.TimeStamp.Add(Now);
                            await Task.Run(async () =>
                            {
                                for (int i = 0; i < 2 & (await server.UpdateMagazineTimeStamp(Now) != 1); i++)
                                {
                                }
                            });
                            await server.UpdateMagazineTimeStamp(Now);
                        }
                        else if (resultCode == 2)
                        {
                            Logger.LogMessage("GJ", "TCP");
                            if (MagazineCounter == true)
                            {
                                LoaderReportData.loaderReport.MagazineId = "SIPMGZ001";
                                MagazineCounter = false;
                            }
                            else
                            {
                                LoaderReportData.loaderReport.MagazineId = "SIPMGZ002";
                                MagazineCounter = true;
                            }
                            if (LoaderReportData.loaderReport.TimeStamp != null && LoaderReportData.loaderReport.TimeStamp.Any())
                            {
                                LoaderReportData.loaderReport.StartTime = LoaderReportData.loaderReport.TimeStamp.First();
                                LoaderReportData.loaderReport.EndTime = LoaderReportData.loaderReport.TimeStamp.Last();
                            }
                            string update = JsonConvert.SerializeObject(LoaderReportData.loaderReport);
                            LoaderReportData.loaderReport = new LoaderReport();
                            await Task.Run(async () =>
                            {
                                for (int i = 0; i < 2 & (await server.UpdateMagazineReport(update)) != 1; i++)
                                {
                                }
                            });
                            
                        }
                        else if (resultCode == 3)
                        {
                            Logger.LogMessage("Connection is aborted", "error");
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogMessage("Unexpected error happen", "error");
                    Console.WriteLine(ex.ToString());
                }
            }
        }
    }
}
