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

        [HttpPost()]
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
                        for (int i = 0; i<5 & !_tcp.ConnectTcp(_modeConfiguration.ServerIP, _modeConfiguration.ServerPort.ToString()) ; i++)
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
                            for (int i = 0 ; i<5 & (resultCode = _tcp.SendRecipe(1)) != 1; i++)
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
                        _taskSyncService.ProceedTaskSource.TrySetResult(true);
                        _taskSyncService.Reset();
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
    }
}
