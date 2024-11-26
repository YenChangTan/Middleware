using Microsoft.Extensions.Primitives;
using Middleware.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Middleware.Controller
{
    public class BLLServerForOPC
    {
        private static readonly HttpClient MESClient = new HttpClient();
        public string previousStatus;
        public DateTime InTime = DateTime.Now;
        private static string baseAddress { get; set; }
        public static void SetBaseAddress(string ipAddress, int Port)
        {
            baseAddress = $"http://{ipAddress}:{Port}/";
        }

        public void SetBearerToken(string token)
        {
            MESClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        public static void SetMESTimeOut(int Timeout)
        {
            MESClient.Timeout = TimeSpan.FromMilliseconds(Timeout);
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
                if (previousStatus == "Busy" && Status == "Free")
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
                    if (previousStatus == "Free" && Status == "Busy")
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
                return 0;//request fail, data corrupted.
            }
        }
    }
}
