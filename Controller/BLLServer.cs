using Middleware.Model;
using Middleware.Model.AMR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Middleware.DataHolder;
using Microsoft.Extensions.Logging;

namespace Middleware.Controller
{
    public class BLLServer
    {
        public string previousStatus;

        public DateTime InTime = DateTime.Now;

        private static readonly HttpClient MESClient = new HttpClient();

        private static readonly HttpClient deviceClient = new HttpClient();

        private static string baseAddress { get; set; }
        private static string deviceAddress { get; set; }

        /*public BLLServer(string ipAddress, string Port)
        {
            //client.BaseAddress = new Uri($"http://{ipAddress}:{Port}/");
            baseAddress = $"http://{ipAddress}/{Port}/";
        }*/

        public static void SetBaseAddress(string ipAddress, int Port)
        {
            baseAddress = $"http://{ipAddress}:{Port}/";
        }

        public static void setDeviceAddress(string ipAddress, int Port, string endpoint = "")
        {
            deviceAddress = $"http://{ipAddress}:{Port}/{endpoint}";
        }

        public static void SetMESTimeOut(int Timeout)
        {
            MESClient.Timeout = TimeSpan.FromMilliseconds(Timeout);
        }

        public static void SetDeviceTimeOut(int Timeout)
        {
            deviceClient.Timeout = TimeSpan.FromMilliseconds(Timeout);
        }

        public static void SetBearerToken(string token)
        {
            MESClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<int> GetPCBStatus(string Barcode)
        {
            string urlEndpoint = $"{baseAddress}api/Workflow";
            PCBStatusRequest pcbStatusRequest = new PCBStatusRequest();
            try
            {
                pcbStatusRequest.Barcode = Barcode;
                string jsonData = JsonConvert.SerializeObject(pcbStatusRequest);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await MESClient.PostAsync(urlEndpoint, content);
                if (!response.IsSuccessStatusCode)
                {
                    return 3;
                }
                string responseBody = await response.Content.ReadAsStringAsync();
                PCBStatusResult pcbStatusResult = new PCBStatusResult();
                pcbStatusResult = JsonConvert.DeserializeObject<PCBStatusResult>(responseBody);
                if (pcbStatusResult.HasResult)
                {
                    if (pcbStatusResult.Status == "Good")
                    {
                        return 0; //request success
                    }
                    else if (pcbStatusResult.Status == "Bad")
                    {
                        return 1; //request success
                    }
                    else
                    {
                        return 3; //data corrupted since the return result is true but the return status is not Good or Bad
                    }
                }
                else
                {
                    return 2;//pcbnotfound
                }
            }
            catch( Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return 3;//request fail, data corrupted.
                //BigChangus.Write(TO_INT())
            }
        }

        public async Task<int> UpdateMachineStatus(string machineName, string Status)
        {
            string urlEndpoint = $"{baseAddress}api/Workflow";
            MachineStatusUpdate machineStatusUpdate = new MachineStatusUpdate();
            try
            {
                machineStatusUpdate.TaskName = machineName;
                machineStatusUpdate.MachineStatus = Status;
                if (previousStatus == Status)
                {
                    return 1;
                }
                if (previousStatus == "Busy" & Status == "Free")
                {
                    machineStatusUpdate.InTime = InTime;
                    machineStatusUpdate.OutTime = DateTime.Now;
                }
                else
                {
                    machineStatusUpdate.InTime = DateTime.Now;
                }
                string jsonData = JsonConvert.SerializeObject(machineStatusUpdate);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await MESClient.PostAsync(urlEndpoint, content);
                if (!response.IsSuccessStatusCode)
                {
                    return 0; //request fail, data corrupted.
                }
                string responseBody = await response.Content.ReadAsStringAsync();
                MachineStatusUpdateResult machineStatusUpdateResult = new MachineStatusUpdateResult();
                machineStatusUpdateResult = JsonConvert.DeserializeObject<MachineStatusUpdateResult>(responseBody);
                if (machineStatusUpdateResult.HasResult)
                {
                    if (previousStatus == "Free" & Status == "Busy")
                    {
                        InTime = machineStatusUpdate.InTime;
                    }
                    previousStatus = Status;
                    return 1;
                }
                else
                {
                    return 0; //Not sure how to define this scenario, update unsuccessfully?
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return 0;//request fail, data corrupted.
            }
        }

        public async Task<int> UpdateMagazineTimeStamp(DateTime dateTime)
        {
            string urlEndpoint = $"{baseAddress}api/Workflow";
            MachineStatusUpdate machineStatusUpdate = new MachineStatusUpdate();
            try
            {
                machineStatusUpdate.TaskName = "Magazine Loader";
                machineStatusUpdate.InTime = dateTime;
                string jsonData = JsonConvert.SerializeObject(machineStatusUpdate);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await MESClient.PostAsync(urlEndpoint, content);
                if (!response.IsSuccessStatusCode)
                {
                    Logger.LogMessage($"api request fail {response.StatusCode}", "error");
                    return 0; //request fail, data corrupted.
                }
                string responseBody = await response.Content.ReadAsStringAsync();
                MachineStatusUpdateResult machineStatusUpdateResult = new MachineStatusUpdateResult();
                machineStatusUpdateResult = JsonConvert.DeserializeObject<MachineStatusUpdateResult>(responseBody);
                if (machineStatusUpdateResult.HasResult)
                {
                    return 1;
                }
                else
                {
                    Logger.LogMessage("api request fail", "error");
                    return 0; //Not sure how to define this scenario, update unsuccessfully?
                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), "error");
                return 0;
            }
        }

        public async Task<int> UpdateMagazineReport(string LoaderReport)//Magazine Loader
        {
            string urlEndpoint = $"{baseAddress}api/Workflow";
            MachineStatusUpdate machineStatusUpdate = new MachineStatusUpdate();
            try
            {
                machineStatusUpdate.TaskName = "Magazine Loader";
                machineStatusUpdate.MachineStatus = "Report";
                machineStatusUpdate.RawData = LoaderReport;
                string jsonData = JsonConvert.SerializeObject(machineStatusUpdate);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await MESClient.PostAsync(urlEndpoint, content);
                if (!response.IsSuccessStatusCode)
                {
                    Logger.LogMessage("update report fail", "error");
                    return 0; //request fail, data corrupted.
                }
                string responseBody = await response.Content.ReadAsStringAsync();
                MachineStatusUpdateResult machineStatusUpdateResult = new MachineStatusUpdateResult();
                machineStatusUpdateResult = JsonConvert.DeserializeObject<MachineStatusUpdateResult>(responseBody);
                if (machineStatusUpdateResult.HasResult)
                {
                    Logger.LogMessage("update report successfully", "api");
                    return 1;
                }
                else
                {
                    Logger.LogMessage("update report fail", "error");
                    return 0; //Not sure how to define this scenario, update unsuccessfully?
                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage($"update report fail {ex.ToString()}", "error");
                return 0;//request fail, data corrupted.
            }
        }

        public async Task<int> createAGVTask(CreateTask createTask)
        {
            string urlEndpoint = $"{deviceAddress}v1/external/dispatch/createTask";
            try
            {
                string jsonData = JsonConvert.SerializeObject(createTask);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await deviceClient.PostAsync(urlEndpoint, content);
                if (!response.IsSuccessStatusCode)
                {
                    return 0; //request fail, data corrupted.
                }
                string responseBody = await response.Content.ReadAsStringAsync();
                CreateTaskReturn createTaskReturn= JsonConvert.DeserializeObject<CreateTaskReturn>(responseBody);
                if (createTaskReturn.status != 200 | createTaskReturn.status == null)
                {
                    return 0;
                }
                bool isInAMRTaskList = false;
                foreach (var task in AMRTaskList.taskDetailsForMEs)
                {
                    if (task.orderId == createTaskReturn.content.orderId)
                    {
                        task.taskId = createTaskReturn.content.taskId;
                        task.isDone = false;
                        isInAMRTaskList = true;
                        break;
                    }
                }
                if (!isInAMRTaskList)
                {
                    TaskDetailsForMES taskDetailsForMES = new TaskDetailsForMES()
                    {
                        taskId = createTaskReturn.content.taskId,
                        orderId = createTaskReturn.content.orderId
                    };
                    AMRTaskList.taskDetailsForMEs.Add(taskDetailsForMES);
                }
                return 1;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public async Task<int> CheckAGVTaskStatus(string orderId)
        {
            try
            {
                string urlEndpoint = $"{deviceAddress}v1/external/dispatch/findByOrderId?id={orderId}";
                HttpResponseMessage response = await deviceClient.GetAsync(urlEndpoint);
                if (!response.IsSuccessStatusCode)
                {
                    return 0; //request fail, data corrupted.
                }
                string responseBody = await response.Content.ReadAsStringAsync();
                CreateTaskReturn createTaskReturn = JsonConvert.DeserializeObject<CreateTaskReturn>(responseBody);
                if (createTaskReturn.status != 200 | createTaskReturn.status == null)
                {
                    return 0;
                }
                if (createTaskReturn.content.taskStatus == 3 | createTaskReturn.content.taskStatus == 5)
                {
                    return 1;//task done
                }
                else
                {
                    return 2;//task not done
                }
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
    }
}
