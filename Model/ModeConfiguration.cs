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
        public string ClientIP { get; set; }
        public int ClientPort { get; set; }
        public string ServerIP { get; set; }
        public int ServerPort { get; set; }
        public string MESIP {  get; set; }
        public int MESPort { get; set; }
        public string MESToken { get; set; }
        public int TimeOut { get; set; }

        // Optional: for OPC-related settings
        //public Dictionary<string, string> OpcNodeIds { get; set; }
    }
}
