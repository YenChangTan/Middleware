using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Middleware.Controller;
using Middleware.DataHolder;
using Middleware.Fundamental;
using Middleware.Model;
using Opc.Ua;

namespace Middleware.BackgroundTask
{
    public class OPCClientService : BackgroundService
    {
        public readonly ModeConfiguration _modeConfiguration = new ModeConfiguration();
        public readonly OPCUA _opc = new OPCUA();
        public OPCClientService(ModeConfiguration modeConfiguration, OPCUA opc)
        {
            _modeConfiguration = modeConfiguration;
            if (modeConfiguration.RobotName != "OPCLine1"){
                _opc = opc;
            };
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Your TCP server logic here
            if (_modeConfiguration.RobotName == "OPCLine1")
            {
                foreach (var opcConnection in OPCConnections.opcConnections)
                {
                    _ = HandleOPC(opcConnection);
                }
            }
            else
            {
                _ = HandleIFMOPC(_opc);
            }
            
        }

        public async Task HandleIFMOPC(OPCUA opcClient)
        {
            OPCUA _opcClient = opcClient;
            DataValue dataValue = new DataValue();
            BLLServerForOPC bllServer1 = new BLLServerForOPC();
            BLLServerForOPC bllServer2 = new BLLServerForOPC();
            BLLServerForOPC bllServer4 = new BLLServerForOPC();
            bllServer1.SetBearerToken("XQMufDx8kg3w8rAt5DUhQY1Pz1V0O0e6gfI6g9cMM/nUbFvlxy3Nav2KPssqVajn");
            bllServer2.SetBearerToken("aOB8XQYQiNE7nL0D0AG06gIyOBPTjXz8xDS1BrO3sa6Ixk8LSqgC56shfV5hR2w6");
            bllServer4.SetBearerToken("K/wk9Mz69ooEMriTzMShf89NGkZ/WfEdf44Sp3yQmtVq5QJfR2esEKZym4k+Hf8V");
            while (true)
            {
                try
                {
                    await _opcClient.LoadConfig();
                    while (!(await _opcClient.OpcConnectProcessAsync(_modeConfiguration.Server.FirstOrDefault().IP, _modeConfiguration.Server.FirstOrDefault().Port.ToString())).result)
                    {
                        await Task.Delay(5000);
                    }
                    while (true)
                    {
                        await Task.Delay(2000);
                        dataValue = await _opcClient.OpcReadAsync($"{_modeConfiguration.OPC.FirstOrDefault().NodeBase}Status1");
                        if (dataValue.StatusCode == StatusCodes.Bad)
                        {
                            break;
                        }
                        else
                        {
                            string Status = null;
                            switch (dataValue.Value.ToString())
                            {
                                case "busy":
                                    Status = "Busy";
                                    break;
                                case "free":
                                    Status = "Free";
                                    break;
                                default:
                                    Status = "Error";
                                    break;
                            }
                            if (Status == null)
                            {
                                continue;
                            }
                            
                            Console.WriteLine($"Conveyor is {Status}");
                            _ = bllServer1.UpdateMachineStatus("IFM 1", Status);
                            //need update to MES
                        }
                        dataValue = await _opcClient.OpcReadAsync($"{_modeConfiguration.OPC.FirstOrDefault().NodeBase}Status3");
                        if (dataValue.StatusCode == StatusCodes.Bad)
                        {
                            break;
                        }
                        else
                        {
                            string Status = null;
                            switch (dataValue.Value.ToString())
                            {
                                case "busy":
                                    Status = "Busy";
                                    break;
                                case "free":
                                    Status = "Free";
                                    break;
                                default:
                                    Status = "Error";
                                    break;
                            }
                            if (Status == null)
                            {
                                continue;
                            }
                            
                            Console.WriteLine($"Conveyor is {Status}");
                            _ = bllServer2.UpdateMachineStatus("IFM 2", Status);
                            //need update to MES
                        }
                        dataValue = await _opcClient.OpcReadAsync($"{_modeConfiguration.OPC.FirstOrDefault().NodeBase}Status4");
                        if (dataValue.StatusCode == StatusCodes.Bad)
                        {
                            break;
                        }
                        else
                        {
                            string Status = null;
                            switch (dataValue.Value.ToString())
                            {
                                case "busy":
                                    Status = "Busy";
                                    break;
                                case "free":
                                    Status = "Free";
                                    break;
                                default:
                                    Status = "Error";
                                    break;
                            }
                            if (Status == null)
                            {
                                continue;
                            }
                            Console.WriteLine($"Conveyor is {Status}");
                            _ = bllServer4.UpdateMachineStatus("IFM 3", Status);
                            //need update to MES

                        }
                        dataValue = await _opcClient.OpcReadAsync($"{_modeConfiguration.OPC.FirstOrDefault().NodeBase}ConveyerStatus");
                        if (dataValue.StatusCode == StatusCodes.Bad)
                        {
                            break;
                        }
                        else
                        {
                            string Status = null;
                            switch (dataValue.Value.ToString())
                            {
                                case "busy":
                                    Status = "Busy";
                                    break;
                                case "free":
                                    Status = "Free";
                                    break;
                                default:
                                    Status = "Error";
                                    break;
                            }
                            if (Status == null)
                            {
                                continue;
                            }
                            //bllServer.SetBearerToken();
                            //need to add Conveyor here.
                            //need update to MES

                        }
                    }
                }
                catch(Exception ex)
                {
                    Logger.LogMessage($"Unexpected Error: {ex.Message}", "error");
                }

            }
        }

        public async Task HandleOPC(OPCConnection opcConnection)
        {
            OPCConnection _opcConnection = opcConnection;
            DataValue dataValue = new DataValue();
            bool HomingBool = true;
            string OPCName = null;
            string IP = null ;
            string Port = null;
            string Token = null;
            string NodeBase = null; ;
            foreach (var opcInfo in _modeConfiguration.OPC)
            {
                
                if (opcInfo.OPCName == _opcConnection.OPCName)
                {
                    IP = opcInfo.ServerIP;
                    Port = opcInfo.ServerPort;
                    Token = opcInfo.Token;
                    NodeBase = opcInfo.NodeBase;
                }
            }
            while (true)
            {
                try
                {
                    _opcConnection.bllServerForOPC.SetBearerToken(Token);
                    await _opcConnection.OPCClient.LoadConfig();
                }
                catch (Exception ex)
                {
                    continue;
                }
                try
                {
                    while (!(await _opcConnection.OPCClient.OpcConnectProcessAsync(IP, Port)).result)
                    {
                        
                        Console.WriteLine($"try to reconnect to {_opcConnection.OPCName}");
                    }
                    _opcConnection.isConnected = true;
                    if (HomingBool)
                    {
                        if (!(await _opcConnection.OPCClient.OpcWriteAsync($"{NodeBase}Global.callWidth_Home", true)).result)
                        {
                            _opcConnection.isConnected = false;
                            continue;
                        }
                        bool isHomingDone = false;
                        do
                        {
                            dataValue = await _opcConnection.OPCClient.OpcReadAsync($"{NodeBase}OPC_read_status.Homing_Done");
                            if (dataValue.StatusCode == StatusCodes.Bad)
                            {
                                _opcConnection.isConnected = false;
                                break;
                            }
                            isHomingDone = dataValue.Value is bool value && value;
                            Console.WriteLine(isHomingDone);
                            await Task.Delay(500);
                        } while (!isHomingDone) ;
                        if (dataValue.StatusCode == StatusCodes.Bad)
                        {
                            _opcConnection.isConnected = false;
                            continue;
                        }
                        if (!(await _opcConnection.OPCClient.OpcWriteAsync($"{NodeBase}Global.callWidth_Home", false)).result)
                        {
                            _opcConnection.isConnected = false;
                            continue;
                        }
                        bool isWidthInPosition = false;
                        do
                        {
                            dataValue = await _opcConnection.OPCClient.OpcReadAsync($"{NodeBase}OPC_read_status.width_in_position");
                            if (dataValue.StatusCode == StatusCodes.Bad)
                            {
                                _opcConnection.isConnected = false;
                                break;
                            }
                            isWidthInPosition = dataValue.Value is bool value && value;
                            await Task.Delay(500);
                        } while (!isWidthInPosition);
                        if (dataValue.StatusCode == StatusCodes.Bad)
                        {
                            _opcConnection.isConnected = false;
                            continue;
                        }
                        HomingBool = false;
                    }

                    if (HomingBool)
                    {
                        _opcConnection.isConnected = false;
                        continue;
                    }
                    while (true)
                    {
                        await Task.Delay(1000);
                        dataValue = await _opcConnection.OPCClient.OpcReadAsync($"{NodeBase}Global.MachineStatus");
                        if (dataValue.StatusCode == StatusCodes.Bad)
                        {
                            _opcConnection.isConnected = false;
                            break;
                        }
                        else
                        {
                            string Status = null;
                            switch (dataValue.Value.ToString())
                            {
                                case "0":
                                    Status = "Busy";
                                    break;
                                case "1":
                                    Status = "Free";
                                    break;
                                default:
                                    Status = "Error";
                                    break;
                            }
                            if (Status == null)
                            {
                                continue;
                            }
                            Console.WriteLine(_opcConnection.OPCName + $"is {Status}");
                            //await _opcConnection.bllServerForOPC.UpdateMachineStatus(_opcConnection.OPCName, Status);

                        }
                    }
                }
                catch(Exception ex)
                {

                }
            }
            
        }
    }
}
