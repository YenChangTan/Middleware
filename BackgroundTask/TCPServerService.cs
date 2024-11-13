using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.Hosting;
using Middleware.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Middleware.Controller;
//using System.BigChangus.Lib;
namespace Middleware.BackgroundTask
{
    public class TCPServerService : BackgroundService
    {
        public readonly ModeConfiguration _modeConfiguration = new ModeConfiguration();
        public readonly TaskSyncService _taskSyncService = new TaskSyncService();
        public TCPServerService(ModeConfiguration modeConfiguration, TaskSyncService taskSyncService)
        {
            _modeConfiguration = modeConfiguration;
            _taskSyncService = taskSyncService;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await TCPServer(_modeConfiguration.ClientIP, _modeConfiguration.ClientPort);
        }

        public bool StartRunEndPointExe(string taskName)
        {
            try
            {
                Process.Start("schtasks", $"/RUN /TN \"{taskName}\"");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task TCPServer(string IP,int port)
        {
            int Port = port;
            IPAddress AllowedIPAddress = IPAddress.Parse(IP);
            List<TcpClient> clients = new List<TcpClient>();

            TcpListener listener = new TcpListener(IPAddress.Any, Port);
            listener.Start();
            Console.WriteLine("waiting for connection");
            while (true)
            {
                try
                {
                    TcpClient client = await listener.AcceptTcpClientAsync();
                    lock (clients) clients.Add(client);
                    if (_modeConfiguration.ModeId == "1")
                    {
                        _ = HandleClient1(client);
                    }
                    else if (_modeConfiguration.ModeId == "4")
                    {
                        _ = HandleClient2(client);
                    }
                    else
                    {
                        break;
                    }
                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        public async Task HandleClient1(TcpClient client)//this function handle the client side from ah teoh the magazine loader before the smt line.
        {
            using (client)
            {
                NetworkStream stream = client.GetStream();
                BLLServer server = new BLLServer();
                int DataLength = 4;
                byte[] buffer = new byte[1024];
                byte[] bytesToSend = new byte[DataLength];
                byte[] receiveBytes = new byte[DataLength];
                try
                {
                    while (true)
                    {
                        buffer = new byte[1024];
                        bytesToSend = new byte[DataLength];
                        receiveBytes = new byte[DataLength];
                        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                        if (bytesRead == 0)
                        {
                            break;
                        }
                        if (bytesRead == 4)
                        {
                            if (Encoding.ASCII.GetString(buffer,0,4) == "DNDN")
                            {
                                int updateMachineStatusResult = await server.UpdateMachineStatus(_modeConfiguration.RobotName, "EMPTY");
                                if (updateMachineStatusResult == 0)
                                {
                                    await stream.WriteAsync(Encoding.ASCII.GetBytes("FAIL"));
                                }
                                else
                                {
                                    await stream.WriteAsync(Encoding.ASCII.GetBytes("DNDN"));
                                }
                                var completedTask = await Task.WhenAny(_taskSyncService.ProceedTaskSource.Task, Task.Delay(_modeConfiguration.TimeOut));
                                if (completedTask == _taskSyncService.ProceedTaskSource.Task)
                                {
                                    await stream.WriteAsync(Encoding.ASCII.GetBytes("DONE"));
                                }
                                else
                                {
                                    await stream.WriteAsync(Encoding.ASCII.GetBytes("FAIL"));
                                }
                                
                            }
                            else if (Encoding.ASCII.GetString(buffer,0,4) == "UPUP")
                            {
                                int updateMachineStatusResult = await server.UpdateMachineStatus(_modeConfiguration.RobotName, "FULL");
                                if (updateMachineStatusResult == 0)
                                {
                                    await stream.WriteAsync(Encoding.ASCII.GetBytes("FAIL"));
                                }
                                else
                                {
                                    await stream.WriteAsync(Encoding.ASCII.GetBytes("UPUP"));
                                }
                                var completedTask = await Task.WhenAny(_taskSyncService.ProceedTaskSource.Task, Task.Delay(_modeConfiguration.TimeOut));
                                if (completedTask == _taskSyncService.ProceedTaskSource.Task)
                                {
                                    await stream.WriteAsync(Encoding.ASCII.GetBytes("DONE"));
                                }
                                else
                                {
                                    await stream.WriteAsync(Encoding.ASCII.GetBytes("FAIL"));
                                }
                            }
                            else
                            {
                                await stream.WriteAsync(Encoding.ASCII.GetBytes("FAIL"));
                            }
                        }
                        else
                        {
                            await stream.WriteAsync(Encoding.ASCII.GetBytes("FAIL"));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        public async Task HandleClient2(TcpClient client)//this function handle client side from Epson at station 1.
        {
            using (client)
            {
                NetworkStream stream = client.GetStream();
                BLLServer server = new BLLServer();
                int DataLength = 7; 
                byte[] buffer = new byte[1024];
                byte[] bytesToSendWithoutCRC = new byte[DataLength];
                byte[] byteToSend = new byte[DataLength];
                byte[] receiveBytesWithoutCRC = new byte[DataLength];
                string pcbBarcode = null;
                try
                {
                    while (true)
                    {
                        buffer = new byte[1024];
                        bytesToSendWithoutCRC = new byte[DataLength];
                        byteToSend = new byte[DataLength];
                        receiveBytesWithoutCRC = new byte[DataLength];
                        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                        if (bytesRead == 0)
                        {
                            break;
                        }
                        if (bytesRead == DataLength)
                        {
                            //Array.Copy(buffer, receiveBytesWithoutCRC, DataLength);
                            //Array.Copy(buffer, DataLength, receivedCRC, 0, 2);
                            //if (!receivedCRC.SequenceEqual(CRCCal(receiveBytesWithoutCRC)))
                            //{
                            //    await stream.WriteAsync(AttachCRC(Encoding.ASCII.GetBytes("DATAERR")));
                            //    continue;
                            //}
                            if (Encoding.ASCII.GetString(buffer, 0, DataLength-1) == "STATUS")
                            {
                                int updateMachineStatusResult;
                                switch (buffer[DataLength - 1])
                                {
                                    case (byte)'0'://Free
                                        updateMachineStatusResult = await server.UpdateMachineStatus(_modeConfiguration.RobotName, "Free");
                                        if (updateMachineStatusResult == 1)
                                        {
                                            Array.Copy(buffer, byteToSend, DataLength);
                                            await stream.WriteAsync(byteToSend);
                                        }
                                        else // == 0
                                        {
                                            await stream.WriteAsync(Encoding.ASCII.GetBytes("DATAERR"));
                                        }
                                        break;
                                    case (byte)'1'://Busy
                                        updateMachineStatusResult = await server.UpdateMachineStatus(_modeConfiguration.RobotName, "Busy");
                                        if (updateMachineStatusResult == 1)
                                        {
                                            Array.Copy(buffer, byteToSend, DataLength);
                                            await stream.WriteAsync(byteToSend);
                                        }
                                        else // == 0
                                        {
                                            await stream.WriteAsync(Encoding.ASCII.GetBytes("DATAERR"));
                                        }
                                        break;
                                    case (byte)'2'://Error
                                        updateMachineStatusResult = await server.UpdateMachineStatus(_modeConfiguration.RobotName, "Error");
                                        if (updateMachineStatusResult == 1)
                                        {
                                            Array.Copy(buffer, byteToSend, DataLength);
                                            await stream.WriteAsync(byteToSend);
                                        }
                                        else // == 0
                                        {
                                            await stream.WriteAsync(Encoding.ASCII.GetBytes("DATAERR"));
                                        }
                                        break;
                                    default:
                                        await stream.WriteAsync(Encoding.ASCII.GetBytes("DATAERR"));
                                        break;
                                }
                            }
                            else
                            {
                                if (Encoding.ASCII.GetString(buffer,0, DataLength) == "DOLASER")
                                {
                                    Array.Copy(buffer, byteToSend, DataLength);
                                    await stream.WriteAsync(byteToSend);
                                    //need to add update tho
                                    var completedTask = await Task.WhenAny(_taskSyncService.ProceedTaskSource.Task, Task.Delay(_modeConfiguration.TimeOut));
                                    if (completedTask == _taskSyncService.ProceedTaskSource.Task)
                                    {
                                        await stream.WriteAsync(Encoding.ASCII.GetBytes("TASKEND"));
                                    }
                                    else
                                    {
                                        await stream.WriteAsync(Encoding.ASCII.GetBytes("DATAERR"));
                                    }
                                }
                                else
                                {
                                    await stream.WriteAsync(Encoding.ASCII.GetBytes("DATAERR"));
                                }
                            }
                        }
                        else
                        {
                            if (bytesRead == 10 & Encoding.ASCII.GetString(buffer, 0, 6) == "SIPBTS")
                            {
                                pcbBarcode = Encoding.ASCII.GetString(buffer, 0, 10);
                                int pcbStatusResult = await server.GetPCBStatus(pcbBarcode);
                                switch (pcbStatusResult)
                                {
                                    case 0://pcb is under good condition.
                                        Array.Copy(Encoding.ASCII.GetBytes("STATUS"), bytesToSendWithoutCRC, 6);
                                        bytesToSendWithoutCRC[DataLength - 1] = (byte)'0';
                                        await stream.WriteAsync(bytesToSendWithoutCRC);
                                        break;
                                    case 1://pcb is defected.
                                        Array.Copy(Encoding.ASCII.GetBytes("STATUS"), bytesToSendWithoutCRC, 6);
                                        bytesToSendWithoutCRC[DataLength - 1] = (byte)'1';
                                        await stream.WriteAsync(bytesToSendWithoutCRC);
                                        break;
                                    case 2: //pcb not found.
                                        Array.Copy(Encoding.ASCII.GetBytes("STATUS"), bytesToSendWithoutCRC, 6);
                                        bytesToSendWithoutCRC[DataLength - 1] = (byte)'2';
                                        await stream.WriteAsync(bytesToSendWithoutCRC);
                                        break;
                                    default://consider as data corrupted.
                                        await stream.WriteAsync(Encoding.ASCII.GetBytes("DATAERR"));
                                        break;
                                }
                            }
                            else
                            {
                                await stream.WriteAsync(Encoding.ASCII.GetBytes("DATAERR"));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        public byte[] CRCCal(byte[] data)
        {
            ushort crc = 0xFFFF;
            foreach (byte element in data)
            {
                crc ^= element;
                for (int i = 0; i < 8; i++)
                {
                    if ((crc & 0x0001) == 1)
                    {
                        crc >>= 1;
                        crc ^= 0xA001;
                    }
                    else
                    {
                        crc >>= 1;
                    }
                }
            }
            byte[] CRC = { (byte)(crc & 0xFF), (byte)((crc >> 8) & 0xFF) };
            return CRC;
        }

        public byte[] AttachCRC(byte[] bytesToSendWithoutCRC)
        {
            byte[] bytesToSend = new byte[bytesToSendWithoutCRC.Length+2];
            int DataLength = bytesToSendWithoutCRC.Length;
            Array.Copy(bytesToSendWithoutCRC, bytesToSend, DataLength);
            Array.Copy(CRCCal(bytesToSendWithoutCRC), 0, bytesToSend, DataLength, 2);
            return bytesToSend;
        }

        public string ByteArrayToHex(byte[] bytes)
        {
            string separator = " ";
            StringBuilder hex = new StringBuilder(bytes.Length * (2 + separator.Length));
            for (int i = 0; i < bytes.Length; i++)
            {
                if (i > 0)
                {
                    hex.Append(separator);
                }
                hex.AppendFormat("{0:X2}", bytes[i]);
            }
            return hex.ToString();
        }
    }
}
//AttachedCRC(BYTE(receivedMsg));

//IF receivedMsg = BigChangus THEN
//  bBigChangusReceived := TRUE;
//END_IF