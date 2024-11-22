using Microsoft.Extensions.Hosting;
using Middleware.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Middleware.Fundamental;

namespace Middleware.BackgroundTask
{
    public class TCPClientService :BackgroundService
    {
        ModeConfiguration _modeConfiguration = new ModeConfiguration();
        TCP _tcp = new TCP();

        public TCPClientService(ModeConfiguration modeConfiguration, TCP tcp)
        {
            _modeConfiguration = modeConfiguration;
            _tcp = tcp;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Your TCP server logic here
            while (!stoppingToken.IsCancellationRequested)
            {
                // Simulate TCP server listening
                // You can replace this with your actual TCP server logic
                while (!_tcp.ConnectTcp(_modeConfiguration.Server.First().IP, _modeConfiguration.Server.First().Port.ToString()))
                {
                    
                    await Task.Delay(5000);
                }
                Console.WriteLine("Connected successfully");
                while (true)
                {
                    try
                    {
                        //31 31 20 20 20 20 20 20 0D 0A
                        //47 4A 20 20 20 20 20 20 0D 0A
                        string BarcodeInfo = await _tcp.ReceiveString();
                        Console.WriteLine(BarcodeInfo.Length);
                        //need to update the read barcode.
                    }
                    catch
                    {
                        break;
                    }


                }
            }
            Console.WriteLine("TCP Server has stopped.");
        }


    }
}
