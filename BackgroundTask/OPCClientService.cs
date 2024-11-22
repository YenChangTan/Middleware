﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Middleware.BackgroundTask
{
    public class OPCClientService : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
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
    }
}
