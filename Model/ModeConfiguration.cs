using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Middleware.Model
{
    public class ModeConfiguration
    {
        public string ModeId { get; set; }
        public string RobotName { get; set; }
        public string EndpointType { get; set; }  // OPC, TCP, NA
        public string EndpointRole { get; set; }  // Server, Client, NA
        public List<Network> Clients { get; set; }
        public List<Network> Server {  get; set; }
        public string MESIP {  get; set; }
        public int MESPort { get; set; }
        public string MESToken { get; set; }
        public string DeviceIp { get; set; }
        public int DevicePort { get; set; }
        public int MESRequestTimeOut { get; set; }
        public int TimeOut { get; set; }

        // Optional: for OPC-related settings
        //public Dictionary<string, string> OpcNodeIds { get; set; }
    }
}
