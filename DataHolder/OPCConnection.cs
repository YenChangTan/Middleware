using Middleware.Controller;
using Middleware.Fundamental;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Middleware.DataHolder
{
    public static class OPCConnections
    {
        public static List<OPCConnection> opcConnections = new List<OPCConnection>();
    }

    public class OPCConnection
    {
        public string OPCName { get; set; }
        public OPCUA OPCClient { get; set; }
        public string NodeBase { get; set; }
        public int Recipe {  get; set; }
        public BLLServerForOPC bllServerForOPC { get; set; }
        public bool isConnected { get; set; }
        public OPCConnection()
        {
            isConnected = false;
            OPCClient = new OPCUA();
            bllServerForOPC = new BLLServerForOPC();
        }
    }
}
