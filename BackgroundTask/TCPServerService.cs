using Microsoft.Extensions.Hosting;
using Middleware.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Middleware.BackgroundTask
{
    public class TCPServerService : BackgroundService
    {
        ModeConfiguration _modeConfiguration = new ModeConfiguration();
        public TCPServerService(ModeConfiguration modeConfiguration)
        {
            _modeConfiguration = modeConfiguration;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            for (int i = 0; !StartRunEndPointExe(_modeConfiguration.RobotName)| i<5 ; i++)
            {
                Console.WriteLine("Run exe fail, attempt to run again");
                await Task.Delay(1000);
                if (i == 4)
                {
                    Environment.Exit(1);
                }
            }

            // Your TCP server logic here
            while (!stoppingToken.IsCancellationRequested)
            {
                // Simulate TCP server listening
                // You can replace this with your actual TCP server logic
                Console.WriteLine("TCP Server is running...");

                // Wait for a small delay before the next iteration, respecting the cancellation token
                await Task.Delay(1000, stoppingToken);
            }

            // Any cleanup or final logic when the service is stopped
            Console.WriteLine("TCP Server has stopped.");
        }

        public bool StartRunEndPointExe(string taskName)
        {
            try
            {
                Process.Start("schtasks", $"/RUN /TN \"{taskName}\"");
                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
    }
}
