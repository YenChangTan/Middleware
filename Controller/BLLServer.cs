using Middleware.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Middleware.Controller
{
    public class BLLServer
    {
        public string previousStatus = "Free";

        public DateTime InTime = DateTime.Now;

        private static readonly HttpClient client = new HttpClient();

        private static string baseAddress { get; set; }

        /*public BLLServer(string ipAddress, string Port)
        {
            //client.BaseAddress = new Uri($"http://{ipAddress}:{Port}/");
            baseAddress = $"http://{ipAddress}/{Port}/";
        }*/

        public static void SetBaseAddress(string ipAddress, int Port)
        {
            baseAddress = $"http://{ipAddress}:{Port}/";
        }

        public static void SetTimeOut(int Timeout)
        {
            client.Timeout = TimeSpan.FromMilliseconds(Timeout);
        }

        public static void SetBearerToken(string token)
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
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
                HttpResponseMessage response = await client.PostAsync(urlEndpoint, content);
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
                HttpResponseMessage response = await client.PostAsync(urlEndpoint, content);
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
    }
}
