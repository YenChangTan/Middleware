using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Reflection;
using Middleware.Model;
using Middleware.Model.APIControllerModel;
using Middleware.Fundamental;
using Middleware.Model.AMR;
using System.Reflection.PortableExecutable;
using System.Net.Http.Json;
using Middleware.DataHolder;
using Opc.Ua;
using System.Net.Sockets;

namespace Middleware.Controller
{
    [ApiController]
    [Route("api")]
    public class WorkflowController : ControllerBase
    {
        public ModeConfiguration _modeConfiguration { get; set; }
        public TaskSyncService _taskSyncService { get; set; }
        public TCP _tcp { get; set; }
        public OPCUA _opc {  get; set; }
        public string NodeBase { get; set; }
        public WorkflowController(ModeConfiguration modeConfiguration, TaskSyncService taskSyncService, TCP tcp, OPCUA opc)
        {
            _modeConfiguration = modeConfiguration;
            _taskSyncService = taskSyncService;
            _tcp = tcp;
            _opc = opc;
            if (_modeConfiguration.RobotName == "OPCLine2")
            {
                NodeBase = _modeConfiguration.OPC.FirstOrDefault().NodeBase;
            }
            
        }

        [HttpPost("Workflow")]
        public async Task<IActionResult> DoWork()
        {
            using (StreamReader reader = new StreamReader(Request.Body))
            {
                Result result = new Result();
                try
                {
                    string requestBody = await reader.ReadToEndAsync();
                    MESPost mesPost = JsonConvert.DeserializeObject<MESPost>(requestBody);
                    
                    if (mesPost.TaskID == "Recipe")
                    {

                        //for (int i = 0; i < 5 & !_tcp.ConnectTcp(_modeConfiguration.Server.First().IP, _modeConfiguration.Server.First().Port.ToString()); i++)
                        //{
                        //    if (i == 4)
                        //    {
                        //        Console.WriteLine("Try Connect fail");
                        //        result.HasResult = false;
                        //        return Ok(result);
                        //    }
                        //    await Task.Delay(1000);
                        //}
                        Console.WriteLine("Start attempt connecting");
                        bool isConnected = _tcp.ConnectTcp(_modeConfiguration.Server.First().IP, _modeConfiguration.Server.First().Port.ToString());
                        if (!isConnected)
                        {
                            result.HasResult = false;
                            result.Message = "Connect fail";
                            return Ok(result);
                        }
                        Console.WriteLine("Connect successfully");
                        await Task.Delay(500);
                        if (mesPost.Recipe == "1")
                        {
                            int resultCode = 0;
                            for (int i = 0; i < 5 && (resultCode = _tcp.SendRecipeWithLineEnder(1)) != 1; i++)
                            {
                                if (resultCode != 0 | i == 4)
                                {
                                    _tcp.CloseTcp();
                                    result.HasResult = false;
                                    return Ok(result);
                                }
                            }
                            //resultCode = _tcp.SendRecipe(1);
                            //if (resultCode == 1)
                            //{
                            //    result.HasResult = true;
                            //    return Ok(result);
                            //}
                            //else
                            //{
                            //    result.HasResult = false;
                            //    return Ok(result);
                            //}
                            _tcp.CloseTcp();
                            result.HasResult = true;
                            return Ok(result);


                        }
                        else if (mesPost.Recipe == "2")
                        {
                            int resultCode = 0;
                            for (int i = 0; i < 5 && (resultCode = _tcp.SendRecipe(2)) != 1; i++)
                            {
                                if (resultCode != 0 | i == 4)
                                {
                                    _tcp.CloseTcp();
                                    result.HasResult = false;
                                    return Ok(result);
                                }
                            }
                            _tcp.CloseTcp();
                            result.HasResult = true;
                            return Ok(result);

                        }
                        else
                        {
                            _tcp.CloseTcp();
                            result.HasResult = false;
                            return Ok(result);
                        }
                    }
                    else if (mesPost.TaskID == "Proceed")
                    {
                        try
                        {
                            _taskSyncService.ProceedTaskSource.TrySetResult(true);
                            isRobcoStation1CanWork.canWork = true;
                        }
                        finally
                        {
                            _taskSyncService.Reset();
                        }
                        result.HasResult = true;
                        return Ok(result);
                    }
                    else if (mesPost.TaskID == "AGV")
                    {
                        CreateTask createTask = new CreateTask();
                        bool isFound = false;
                        foreach (var taskMapping in AMRTaskMapping.amrTaskMapping)
                        {
                            if (mesPost.Recipe == taskMapping.Key)
                            {
                                createTask.data.deviceId = taskMapping.Value.deviceId;

                                createTask.data.orderId = taskMapping.Key;
                                foreach (var subtask in taskMapping.Value.subtaskInfos)
                                {
                                    if (subtask.type == "route")
                                    {
                                        createTask.data.task.Add(new TaskInfo()
                                        {
                                            type = subtask.type,
                                            name = subtask.name,
                                        });
                                    }
                                    else if (subtask.type == "uploading" | subtask.type == "unloading")
                                    {
                                        createTask.data.task.Add(new TaskInfo()
                                        {
                                            type = subtask.type,
                                            things = new ThingsId()
                                            {
                                                thingsId = taskMapping.Value.thingsId
                                            }
                                        });
                                    }
                                    else
                                    {

                                    }
                                }
                                isFound = true;
                                break;
                            }
                        }
                        if (!isFound)
                        {
                            result.HasResult = false;
                            result.Message = "TaskId not found";
                            return Ok(result);
                        }
                        BLLServer server = new BLLServer();
                        for (int i = 0; i < 5 && await server.createAGVTask(createTask) != 1; i++)
                        {
                            if (i == 4)
                            {
                                result.HasResult = false;
                                return Ok(result);
                            }
                        }
                        result.HasResult = true;
                        return Ok(result);
                    }
                    else if (mesPost.TaskID == "Take Tray")
                    {
                        BLLServer bllServer = new BLLServer();
                        if (bllServer.previousStatus == "Busy")
                        {

                            //wait untill previous status is not busy and need to call yongwei amr side do work.
                            var completedTask = await Task.WhenAny(_taskSyncService.ProceedTaskSource.Task);
                            result.HasResult = true;
                            return Ok(result);
                        }
                        else if (bllServer.previousStatus == "Free")
                        {
                            isRobcoStation1CanWork.canWork = false;
                            if ( (await bllServer.CallAMRTakePlateStation1()) == 1)
                            {
                                
                                result.HasResult = true;
                                return Ok(result);
                            }
                            else
                            {
                                isRobcoStation1CanWork.canWork = true;
                                result.HasResult = false;
                                return Ok(result);
                                
                            }
                        }
                        else
                        {
                            result.HasResult = false;
                            result.Message="Robco in Error State";
                            return Ok(result);
                        }
                    }
                    else
                    {
                        result.HasResult = false;
                        return Ok(result);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    result = new Result()
                    {
                        HasResult = false,
                        Message = ex.Message
                    };
                    return Ok(result);
                }
            }
        }

        [HttpPost("device/agv/reportTaskStatus")]
        public async Task<IActionResult> reportTaskStatus()
        {
            using (StreamReader reader = new StreamReader(Request.Body))
            {
                Result result = new Result();
                try
                {
                    string requestBody = await reader.ReadToEndAsync();
                    ReportTaskStatus reportTaskStatus = JsonConvert.DeserializeObject<ReportTaskStatus>(requestBody);
                    if (reportTaskStatus.taskStatus == "3")
                    {
                        foreach (var task in AMRTaskList.taskDetailsForMEs)
                        {
                            if (task.orderId == reportTaskStatus.orderId)
                            {
                                task.isDone = true;
                                BLLServer server = new BLLServer();
                                MachineStatusUpdate machineStatusUpdate = new MachineStatusUpdate();
                                for (int i = 0; i < 5 && (await server.UpdateMachineStatus(task.orderId, "Done") != 1); i++)//need to update here, put updatemachinestatus is just for temporary.
                                {
                                    if (i == 4)
                                    {
                                        result.HasResult = false;
                                        return Ok(result);
                                    }
                                }
                            }
                        }
                    }
                    result.HasResult = true;
                    return Ok(result);
                }
                catch (Exception ex)
                {
                    result.HasResult = false;
                    return Ok(result);
                }
            }
        }

        [HttpPost("device/agv/reportCarStatus")]
        public async Task<IActionResult> reportCarStatus()
        {
            using (StreamReader reader = new StreamReader(Request.Body))
            {
                try
                {
                    string requestBody = await reader.ReadToEndAsync();
                    ReportCarStatus reportCarStatus = JsonConvert.DeserializeObject<ReportCarStatus>(requestBody);
                    if (reportCarStatus.error == 1)
                    {
                        BLLServer server = new BLLServer();
                        Task.Run(() => server.UpdateMachineStatus(reportCarStatus.deviceId, "Error"));
                    }
                    return Ok();
                }
                catch (Exception ex)
                {
                    return Ok();
                }
            }
        }

        [HttpPost("Docking")]
        public async Task<IActionResult> Docking()
        {
            using (StreamReader reader = new StreamReader(Request.Body))
            {
                try
                {
                    DataValue dataValue = new DataValue();
                    Result result = new Result();
                    OPCConnection _opcConnection = new OPCConnection();
                    string requestBody = await reader.ReadToEndAsync();
                    MESPost mesPost = JsonConvert.DeserializeObject<MESPost>(requestBody);
                    if (!mesPost.TaskID.Contains("IFM"))
                    {
                        if (!HomingDone.isHomingDone)
                        {
                            result.Message = "Homing is not finished";
                            return Ok(result);
                        }
                        bool isConnectionFound = false;
                        foreach (var opcConnection in OPCConnections.opcConnections)
                        {
                            if (opcConnection.OPCName == mesPost.TaskID)
                            {
                                _opcConnection = opcConnection;
                                isConnectionFound = opcConnection.isConnected;
                                break;
                            }
                        }
                        if (!isConnectionFound)
                        {
                            result.Message = "Connection not found";
                            return Ok(result);
                        }
                        if (!(await _opcConnection.OPCClient.OpcWriteAsync(_opcConnection.NodeBase + "Global.dockingByMES", true)).result)
                        {
                            result.Message = "docking by mes";
                            return Ok(result);
                        }
                        bool isDockingRightDone = false;
                        do
                        {
                            dataValue = await _opcConnection.OPCClient.OpcReadAsync(_opcConnection.NodeBase + "OPC_read_status.docking_RIGHT_Done");
                            if (dataValue.StatusCode == StatusCodes.Bad)
                            {
                                result.Message = "docking right done";
                                return Ok(result);
                            }
                            isDockingRightDone = dataValue.Value is bool value && value;
                        } while (!isDockingRightDone);
                        if (_opcConnection.Recipe == 2)
                        {
                            bool isDockingLeftDone = false;
                            do
                            {
                                dataValue = await _opcConnection.OPCClient.OpcReadAsync(_opcConnection.NodeBase + "OPC_read_status.docking_LEFT_Done");
                                if (dataValue.StatusCode == StatusCodes.Bad)
                                {
                                    result.Message = "docking left done";
                                    return Ok(result);
                                }
                                isDockingLeftDone = dataValue.Value is bool value && value;
                            } while (!isDockingLeftDone);
                        }
                        if (!(await _opcConnection.OPCClient.OpcWriteAsync(_opcConnection.NodeBase + "Global.dockingByMES", false)).result)
                        {
                            result.Message = "docking by mes false";
                            return Ok(result);
                        }
                        if (!(await _opcConnection.OPCClient.OpcWriteAsync(_opcConnection.NodeBase + "Global.connectorOut_ByMES", true)).result)
                        {
                            result.Message = "connector out by mes";
                            return Ok(result);
                        }
                        bool isConnectorRightDone = false;
                        do
                        {
                            dataValue = await _opcConnection.OPCClient.OpcReadAsync(_opcConnection.NodeBase + "OPC_read_status.connector_RIGHT_Done");
                            if (dataValue.StatusCode == StatusCodes.Bad)
                            {
                                result.Message = "connector right done";
                                return Ok(result);
                            }
                            isConnectorRightDone = dataValue.Value is bool value && value;
                        } while (!isConnectorRightDone);
                        if (_opcConnection.Recipe == 2)
                        {
                            bool isConnectorLeftDone = false;
                            do
                            {
                                dataValue = await _opcConnection.OPCClient.OpcReadAsync(_opcConnection.NodeBase + "OPC_read_status.connector_LEFT_Done");
                                if (dataValue.StatusCode == StatusCodes.Bad)
                                {
                                    return Ok(result);
                                }
                                isConnectorLeftDone = dataValue.Value is bool value && value;
                            } while (!isConnectorLeftDone);
                        }
                        result.HasResult = true;
                        return Ok(result);
                    }
                    else
                    {
                        if (mesPost.TaskID.Contains("1"))
                        {
                            if ((await _opc.OpcWriteAsync(NodeBase + "Station1Lock", true)).result)
                            {
                                
                                result.HasResult = true;
                                return Ok(result);
                            }
                            else
                            {
                                Logger.LogMessage($"Error in docking station 1", "error");
                                return Ok(result);
                            }
                        }
                        else if (mesPost.TaskID.Contains("2"))
                        {
                            if ((await _opc.OpcWriteAsync(NodeBase + "Station2Lock", true)).result)
                            {
                                result.HasResult = true;
                                return Ok(result);
                            }
                            else
                            {
                                Logger.LogMessage($"Error in docking station 2", "error");
                                return Ok(result);
                            }
                        }
                        else if (mesPost.TaskID.Contains("3"))
                        {
                            if ((await _opc.OpcWriteAsync(NodeBase + "Station4Lock", true)).result)
                            {
                                result.HasResult = true;
                                return Ok(result);
                            }
                            else
                            {
                                Logger.LogMessage($"Error in docking station 4", "error");
                                return Ok(result);
                            }
                        }
                        else
                        {
                            return Ok(result);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogMessage($"Error in API receiving docking command", "error");
                    Result result = new Result();
                    result.HasResult = false;
                    result.Message = ex.Message;
                    return Ok(result);
                }
            }
        }

        [HttpPost("Undocking")]
        public async Task<IActionResult> Undocking()
        {
            using (StreamReader reader = new StreamReader(Request.Body))
            {
                try
                {
                    DataValue dataValue = new DataValue();
                    Result result = new Result();
                    OPCConnection _opcConnection = new OPCConnection();
                    string requestBody = await reader.ReadToEndAsync();
                    MESPost mesPost = JsonConvert.DeserializeObject<MESPost>(requestBody);
                    if (!mesPost.TaskID.Contains("IFM"))
                    {
                        bool isConnectionFound = false;
                        foreach (var opcConnection in OPCConnections.opcConnections)
                        {
                            _opcConnection = opcConnection;
                            if (opcConnection.OPCName == mesPost.TaskID)
                            {
                                isConnectionFound = true;
                                _opcConnection = opcConnection;
                            }
                        }
                        if (!isConnectionFound)
                        {
                            return Ok(result);
                        }
                        if (!(await _opcConnection.OPCClient.OpcWriteAsync(_opcConnection.NodeBase + "Global.powerOff_WC", true)).result)
                        {
                            result.Message = "power off wc";
                            return Ok(result);
                        }
                        bool isCutOffPower = false;
                        do
                        {
                            dataValue = await _opcConnection.OPCClient.OpcReadAsync(_opcConnection.NodeBase + "OPC_read_status.cutoff_Power");
                            if (dataValue.StatusCode == StatusCodes.Bad)
                            {
                                result.Message = "cut off power";
                                return Ok(result);
                            }
                            isCutOffPower = dataValue.Value is bool value && value;
                        } while (!isCutOffPower);
                        if (!(await _opcConnection.OPCClient.OpcWriteAsync(_opcConnection.NodeBase + "Global.connectorOutByMES", false)).result)
                        {
                            result.Message = "connector out by mes";
                            return Ok(result);
                        }

                        bool isConnectorRightDone = false;
                        do
                        {
                            dataValue = await _opcConnection.OPCClient.OpcReadAsync(_opcConnection.NodeBase + "OPC_read_status.connector_RIGHT_Done");
                            if (dataValue.StatusCode == StatusCodes.Bad)
                            {
                                result.Message = "connectorright done";
                                return Ok(result);
                            }
                            isConnectorRightDone = !(dataValue.Value is bool value && value);
                        } while (!isConnectorRightDone);
                        if (_opcConnection.Recipe == 2)
                        {
                            bool isConnectorLeftDone = false;
                            do
                            {
                                dataValue = await _opcConnection.OPCClient.OpcReadAsync(_opcConnection.NodeBase + "OPC_read_status.connector_LEFT_Done");
                                if (dataValue.StatusCode == StatusCodes.Bad)
                                {
                                    result.Message = "connector left done";
                                    return Ok(result);
                                }
                                isConnectorLeftDone = !(dataValue.Value is bool value && value);
                            } while (!isConnectorLeftDone);
                        }
                        if (!(await _opcConnection.OPCClient.OpcWriteAsync(_opcConnection.NodeBase + "Global.udockingByMES", true)).result)
                        {
                            result.Message = "undocking by mes true";
                            return Ok(result);
                        }

                        bool isUndockingRightDone = false;
                        do
                        {
                            dataValue = await _opcConnection.OPCClient.OpcReadAsync(_opcConnection.NodeBase + "OPC_read_status.undocking_RIGHT_Done");
                            if (dataValue.StatusCode == StatusCodes.Bad)
                            {
                                result.Message = "undocking right done";
                                return Ok(result);
                            }
                            isUndockingRightDone = dataValue.Value is bool value && value;
                        } while (!isUndockingRightDone);
                        if (_opcConnection.Recipe == 2)
                        {
                            bool isUndockingLeftDone = false;
                            do
                            {
                                dataValue = await _opcConnection.OPCClient.OpcReadAsync(_opcConnection.NodeBase + "OPC_read_status.undocking_LEFT_Done");
                                if (dataValue.StatusCode == StatusCodes.Bad)
                                {
                                    result.Message = "undocking left done";
                                    return Ok(result);
                                }
                                isUndockingLeftDone = dataValue.Value is bool value && value;
                            } while (!isUndockingLeftDone);
                        }
                        if (!(await _opcConnection.OPCClient.OpcWriteAsync(_opcConnection.NodeBase + "Global.udockingByMES", false)).result)
                        {
                            result.Message = "undocking by mes";
                            return Ok(result);
                        }
                        if (!(await _opcConnection.OPCClient.OpcWriteAsync(_opcConnection.NodeBase + "Global.powerOff_WC", false)).result)
                        {
                            result.Message = "power off wc false";
                            return Ok(result);
                        }
                        result.HasResult = true;
                        return Ok(result);
                    }
                    else
                    {
                        if (mesPost.TaskID.Contains("1"))
                        {
                            if ((await _opc.OpcWriteAsync(NodeBase + "Station1Lock", false)).result)
                            {
                                result.HasResult = true;
                                return Ok(result);
                            }
                            else
                            {
                                Logger.LogMessage($"Error in undocking station 1", "error");
                                return Ok(result);
                            }
                        }
                        else if (mesPost.TaskID.Contains("2"))
                        {
                            if ((await _opc.OpcWriteAsync(NodeBase + "Station2Lock", false)).result)
                            {
                                result.HasResult = true;
                                return Ok(result);
                            }
                            else
                            {
                                Logger.LogMessage($"Error in undocking station 2", "error");
                                return Ok(result);
                            }
                        }
                        else if (mesPost.TaskID.Contains("3"))
                        {
                            if ((await _opc.OpcWriteAsync(NodeBase + "Station4Lock", false)).result)
                            {
                                result.HasResult = true;
                                return Ok(result);
                            }
                            else
                            {
                                Logger.LogMessage($"Error in undocking station 4", "error");
                                return Ok(result);
                            }
                        }
                        else
                        {
                            return Ok(result);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogMessage($"Error in API receiving docking command", "error");
                    Result result = new Result();
                    result.HasResult = false;
                    result.Message = ex.Message;
                    return Ok(result);
                }
            }
        }
    }
}
