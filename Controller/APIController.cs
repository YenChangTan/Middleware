using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace Middleware.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class APIController: ControllerBase
    {
        public ModeConfiguration _modeConfiguration {  get; set; }
        public TaskSyncService _taskSyncService { get; set; }
        public TCP _tcp { get; set; }
        public APIController(ModeConfiguration modeConfiguration, TaskSyncService taskSyncService, TCP tcp)
        {
            _modeConfiguration = modeConfiguration;
            _taskSyncService = taskSyncService;
            _tcp = tcp;
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
                    MachineStatusUpdate machineStatusUpdate = JsonConvert.DeserializeObject<MachineStatusUpdate>(requestBody);
                    if (machineStatusUpdate.TaskName == "Recipe")
                    {
                        for (int i = 0; i<5 && !_tcp.ConnectTcp(_modeConfiguration.Server.First().IP, _modeConfiguration.Server.First().Port.ToString()) ; i++)
                        {
                            if (i == 4)
                            {
                                Console.WriteLine("Try Connect fail");
                                result.HasResult = false;
                                return Ok(result);
                            }
                            await Task.Delay(1000);
                        }
                        if (machineStatusUpdate.Recipe == "1")
                        {
                            int resultCode = 0;
                            for (int i = 0 ; i<5 && (resultCode = _tcp.SendRecipe(1)) != 1; i++)
                            {
                                if (resultCode != 0 | i == 4)
                                {
                                    result.HasResult = false;
                                    return Ok(result);
                                }
                            }
                            result.HasResult = true;
                            return Ok(result);

                        }
                        else if (machineStatusUpdate.Recipe == "2")
                        {
                            int resultCode = 0;
                            for (int i = 0; i < 5 | (resultCode = _tcp.SendRecipe(2)) != 1; i++)
                            {
                                if (resultCode != 0 | i == 4)
                                {
                                    result.HasResult = false;
                                    return Ok(result);
                                }
                            }
                            result.HasResult = true;
                            return Ok(result);

                        }
                        else
                        {
                            result.HasResult = false;
                            return Ok(result);
                        }
                    }
                    else if (machineStatusUpdate.TaskName == "Proceed")
                    {
                        try
                        {
                            _taskSyncService.ProceedTaskSource.TrySetResult(true);
                        }
                        finally
                        {
                            _taskSyncService.Reset();
                        }
                        result.HasResult = true;
                        return Ok(result);
                    }
                    else if (machineStatusUpdate.TaskName == "AGV")
                    {
                        CreateTask createTask = new CreateTask();
                        bool isFound = false;
                        foreach (var taskMapping in AMRTaskMapping.amrTaskMapping)
                        {
                            if (machineStatusUpdate.Recipe == taskMapping.Key)
                            {
                                createTask.data.deviceId = taskMapping.Value.deviceId;
                                createTask.data.orderId = taskMapping.Key;
                                foreach(var subtask in taskMapping.Value.subtaskInfos)
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
                    else
                    {
                        result.HasResult = false;
                        return Ok(result);
                    }
                }
                catch (Exception ex)
                {
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
    }
}
