using Microsoft.Extensions.Hosting;
using Middleware.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Middleware.BackgroundTask
{
    public class AMRServerService : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            LoaderReport testing = new LoaderReport();
            testing.MagazineId = "SIPMGZ001";
            for (int i = 0; i< 10; i++)
            {
                testing.TimeStamp.Add(DateTime.Now);
                Thread.Sleep(10);
            }
            testing.StartTime = testing.TimeStamp.First();
            testing.EndTime = testing.TimeStamp.Last();
            Console.WriteLine(JsonConvert.SerializeObject(testing));
        }
    }
}
