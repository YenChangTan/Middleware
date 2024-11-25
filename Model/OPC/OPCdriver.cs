using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Middleware.Model.OPC
{
    public class OPCdriver
    {
        public string ServerIP { get; set; }
        public string ServerPort { get; set; }
        public string OPCName { get; set; }
        public int Recipe {  get; set; }
        public string Token { get; set; }
        public string NodeBase { get; set; }
        public List<NodeIds> NodeIds { get; set; }
        public OPCdriver()
        {
            NodeIds = new List<NodeIds>();
        }
    }

    public class NodeIds
    {
        public string NodeName { get; set; }
        public string NodeId { get; set; }
    }
}
