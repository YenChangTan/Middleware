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
using Middleware.DataHolder;
using Newtonsoft.Json;
using System.Text.Json.Nodes;
using Middleware.Fundamental;
//using System.BigChangus.Lib;
namespace Middleware.BackgroundTask
{
    public class TCPServerService : BackgroundService
    {
        public readonly ModeConfiguration _modeConfiguration = new ModeConfiguration();
        public readonly TaskSyncService _taskSyncService = new TaskSyncService();
        public bool MagazineCounter = false;
        public TCPServerService(ModeConfiguration modeConfiguration, TaskSyncService taskSyncService)
        {
            _modeConfiguration = modeConfiguration;
            _taskSyncService = taskSyncService;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //if (_modeConfiguration.Clients.Count() > 1)
            //{
            //    _ = GetTrayBarcode(_modeConfiguration.Clients[1]);
            //}
            _ = GetTrayBarcode(_modeConfiguration.Server[1]);
            await TCPServer(_modeConfiguration.Clients.First().IP, _modeConfiguration.Clients.First().Port);
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
                Logger.LogMessage(ex.Message, "error");
                return false;
            }
        }

        public async Task GetTrayBarcode(Network network)
        {
            BLLServer server = new BLLServer();
            int resultCode = 0;
            TCP _tcp = new TCP();
            while (true)
            {
                try
                {
                    while (!_tcp.ConnectTcp(network.IP, network.Port.ToString()))
                    {
                        await Task.Delay(5000);
                    }

                    while (true)
                    {
                        while (true)
                        {
                            try
                            {

                                string TrayBarcodeInfo = await _tcp.ReceiveString();
                                TrayNPCBData.trayNPCB.TrayID = TrayBarcodeInfo;
                                MachineStatusUpdate machineStatusUpdate = new MachineStatusUpdate();
                                machineStatusUpdate.TaskName = "TrayPCB";
                                machineStatusUpdate.RawData = JsonConvert.SerializeObject(TrayNPCBData.trayNPCB);
                                TrayNPCBData.trayNPCB.PCBIDs = new List<string>();
                            }
                            catch
                            {
                                break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogMessage($"Error in TrayPCB : {ex.Message}", "error");
                }
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
                    //if (_modeConfiguration.ModeId == "1")
                    //{
                    //    _ = HandleClient1(client);
                    //}
                    //else
                    //{
                    //    _ = HandleClient2(client);
                    //}
                    _ = HandleClient2(client);
                    
                }
                catch (Exception ex)
                {
                    Logger.LogMessage($"Error in server flow : {ex.Message}", "error");
                }
            }
        }

        //public async Task HandleClient1(TcpClient client)//this function handle the client side from ah teoh the magazine loader before the smt line.
        //{
        //    using (client)
        //    {
        //        NetworkStream stream = client.GetStream();
        //        BLLServer server = new BLLServer();
        //        int DataLength = 4;
        //        byte[] buffer = new byte[1024];
        //        byte[] bytesToSend = new byte[DataLength];
        //        byte[] receiveBytes = new byte[DataLength];
        //        try
        //        {
        //            while (true)
        //            {
        //                buffer = new byte[1024];
        //                bytesToSend = new byte[DataLength];
        //                receiveBytes = new byte[DataLength];
        //                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
        //                if (bytesRead == 0)
        //                {
        //                    break;
        //                }
        //                if (bytesRead == 4)
        //                {
        //                    if (Encoding.ASCII.GetString(buffer,0,4) == "DNDN")
        //                    {
        //                        int updateMachineStatusResult = await server.UpdateMachineStatus(_modeConfiguration.RobotName, "EMPTY");
        //                        if (updateMachineStatusResult == 0)
        //                        {
        //                            await stream.WriteAsync(Encoding.ASCII.GetBytes("FAIL"));
        //                        }
        //                        else
        //                        {
        //                            await stream.WriteAsync(Encoding.ASCII.GetBytes("DNDN"));
        //                            var completedTask = await Task.WhenAny(_taskSyncService.ProceedTaskSource.Task, Task.Delay(_modeConfiguration.TimeOut));
        //                            if (completedTask == _taskSyncService.ProceedTaskSource.Task)
        //                            {
        //                                await stream.WriteAsync(Encoding.ASCII.GetBytes("DONE"));
        //                            }
        //                            else
        //                            {
        //                                await stream.WriteAsync(Encoding.ASCII.GetBytes("FAIL"));
        //                            }
        //                        }
        //                    }
        //                    else if (Encoding.ASCII.GetString(buffer,0,4) == "UPUP")
        //                    {
        //                        MagazineCounter = !MagazineCounter;
        //                        if (MagazineCounter)
        //                        {
        //                            LoaderReportData.loaderReport.MagazineId = "SIPMGZ001";
                                    
        //                        }
        //                        else
        //                        {
        //                            LoaderReportData.loaderReport.MagazineId = "SIPMGZ002";
                                    
        //                        }
        //                        if (LoaderReportData.loaderReport.TimeStamp != null && LoaderReportData.loaderReport.TimeStamp.Any())
        //                        {
        //                            LoaderReportData.loaderReport.StartTime = LoaderReportData.loaderReport.TimeStamp.First();
        //                            LoaderReportData.loaderReport.EndTime = LoaderReportData.loaderReport.TimeStamp.Last();
        //                        }
        //                        Console.WriteLine(JsonConvert.SerializeObject(LoaderReportData.loaderReport));
        //                        int updateMagazineLoaderResult = await server.UpdateMagazineReport(JsonConvert.SerializeObject(LoaderReportData.loaderReport));
        //                        if (updateMagazineLoaderResult == 0)
        //                        {
        //                            await stream.WriteAsync(Encoding.ASCII.GetBytes("FAIL"));
        //                        }
        //                        else
        //                        {
        //                            //await stream.WriteAsync(Encoding.ASCII.GetBytes("UPUP"));
        //                            //LoaderReportData.loaderReport = new LoaderReport();
        //                            //var completedTask = await Task.WhenAny(_taskSyncService.ProceedTaskSource.Task, Task.Delay(_modeConfiguration.TimeOut));
        //                            //if (completedTask == _taskSyncService.ProceedTaskSource.Task)
        //                            //{
        //                            //    await stream.WriteAsync(Encoding.ASCII.GetBytes("DONE"));
        //                            //}
        //                            //else
        //                            //{
        //                            //    await stream.WriteAsync(Encoding.ASCII.GetBytes("FAIL"));
        //                            //}
        //                            await stream.WriteAsync(Encoding.ASCII.GetBytes("DONE"));
        //                        }
        //                    }
        //                    else if (Encoding.ASCII.GetString(buffer,0,3) == "OUT")
        //                    {
        //                        LoaderReportData.loaderReport.TimeStamp.Add(DateTime.Now);
        //                        Array.Copy(buffer, bytesToSend,4);
        //                        await stream.WriteAsync(bytesToSend);
        //                    }
        //                    else
        //                    {
        //                        await stream.WriteAsync(Encoding.ASCII.GetBytes("FAIL"));
        //                    }
        //                }
        //                else
        //                {
        //                    await stream.WriteAsync(Encoding.ASCII.GetBytes("FAIL"));
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine(ex.ToString());
        //        }
        //    }
        //}

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

                                string Status = null;
                                int updateMachineStatusResult;
                                Array.Copy(buffer, byteToSend, DataLength);
                                await stream.WriteAsync(byteToSend);
                                switch (buffer[DataLength - 1])
                                {
                                    case (byte)'0'://Free
                                        Status = "Free";
                                        updateMachineStatusResult = await server.UpdateMachineStatus(_modeConfiguration.RobotName, "Free");
                                        if (_modeConfiguration.RobotName == "Robco 2")
                                        {
                                            try// this section is use for station 
                                            {
                                                _taskSyncService.ProceedTaskSource.TrySetResult(true);
                                            }
                                            finally
                                            {
                                                _taskSyncService.Reset();
                                            }
                                        }
                                        
                                        //if (updateMachineStatusResult == 1)
                                        //{
                                        //    Array.Copy(buffer, byteToSend, DataLength);
                                        //    await stream.WriteAsync(byteToSend);
                                        //}
                                        //else // == 0
                                        //{
                                        //    await stream.WriteAsync(Encoding.ASCII.GetBytes("DATAERR"));
                                        //}
                                        break;
                                    case (byte)'1'://Busy
                                        Status = "Busy";
                                        //updateMachineStatusResult = await server.UpdateMachineStatus(_modeConfiguration.RobotName, "Busy");
                                        //if (updateMachineStatusResult == 1)
                                        //{
                                        //    Array.Copy(buffer, byteToSend, DataLength);
                                        //    await stream.WriteAsync(byteToSend);
                                        //}
                                        //else // == 0
                                        //{
                                        //    await stream.WriteAsync(Encoding.ASCII.GetBytes("DATAERR"));
                                        //}
                                        break;
                                    default:
                                        Status = "Error";

                                        //updateMachineStatusResult = await server.UpdateMachineStatus(_modeConfiguration.RobotName, "Error");
                                        //if (updateMachineStatusResult == 1)
                                        //{
                                        //    Array.Copy(buffer, byteToSend, DataLength);
                                        //    await stream.WriteAsync(byteToSend);
                                        //}
                                        //else // == 0
                                        //{
                                        //    await stream.WriteAsync(Encoding.ASCII.GetBytes("DATAERR"));
                                        //}
                                        break;
                                }
                                _ = Task.Run(async()=>
                                {
                                    for (int i = 0; i < 5 & (await server.UpdateMachineStatus(_modeConfiguration.RobotName, Status)) != 1; i++)
                                    {
                                    }
                                });
                            }
                            else if (Encoding.ASCII.GetString(buffer,0, DataLength) == "DOLASER")
                            {
                                Array.Copy(buffer, byteToSend, DataLength);
                                await stream.WriteAsync(byteToSend);
                                int updateMachineStatusResult = 0;
                                //need to add update tho
                                for (int i = 0; i < 5 & (updateMachineStatusResult = await server.UpdateMachineStatus(_modeConfiguration.RobotName, "can pick")) != 1; i++)
                                {
                                }
                                if (updateMachineStatusResult == 1)
                                {
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
                            else if (Encoding.ASCII.GetString(buffer, 0, DataLength) == "CANPICK")
                            {
                                int updateMachineStatusResult;
                                //need to add to update to MES, confirm JSON Body
                                for (int i = 0; i < 5 & (updateMachineStatusResult = await server.UpdateMachineStatus(_modeConfiguration.RobotName, "can pick")) != 1; i++)
                                {
                                }
                                if (updateMachineStatusResult == 1)
                                {
                                    Array.Copy(buffer, byteToSend, DataLength);
                                    await stream.WriteAsync(byteToSend);
                                }
                                else
                                {
                                    await stream.WriteAsync(Encoding.ASCII.GetBytes("DATAERR"));
                                }
                            }
                            else if (Encoding.ASCII.GetString(buffer, 0, DataLength) == "CANWORK")
                            {
                                if (isRobcoStation1CanWork.canWork)
                                {
                                    await stream.WriteAsync(Encoding.ASCII.GetBytes("CANWORK"));
                                }
                                else
                                {
                                    await stream.WriteAsync(Encoding.ASCII.GetBytes("NOTWORK"));
                                    var completedTask = await Task.WhenAny(_taskSyncService.ProceedTaskSource.Task);
                                    await stream.WriteAsync(Encoding.ASCII.GetBytes("CANWORK"));
                                }
                            }
                            else
                            {
                                await stream.WriteAsync(Encoding.ASCII.GetBytes("DATAERR"));
                            }
                        }
                        else
                        {
                            if (bytesRead == 10 & Encoding.ASCII.GetString(buffer, 0, 6) == "SIPBTS")
                            {
                                pcbBarcode = Encoding.ASCII.GetString(buffer, 0, 10);
                                int pcbStatusResult = 3;
                                for (int i = 0; i<5 & (pcbStatusResult = await server.GetPCBStatus(pcbBarcode)) == 3; i++)
                                {
                                }
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
                                        TrayNPCBData.trayNPCB.PCBIDs.Add(pcbBarcode);
                                        break;
                                    default: //pcb not found or data error.
                                        Array.Copy(Encoding.ASCII.GetBytes("STATUS"), bytesToSendWithoutCRC, 6);
                                        bytesToSendWithoutCRC[DataLength - 1] = (byte)'2';
                                        await stream.WriteAsync(bytesToSendWithoutCRC);
                                        break;
                                }
                            }
                            else
                            {
                                Array.Copy(Encoding.ASCII.GetBytes("STATUS"), bytesToSendWithoutCRC, 6);
                                bytesToSendWithoutCRC[DataLength - 1] = (byte)'2';
                                await stream.WriteAsync(bytesToSendWithoutCRC);
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