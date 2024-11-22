using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Middleware.Model
{
    public class TrayNPCB
    {
        public string TrayID { get; set; }
        public List<string> PCBIDs { get; set; }

        public TrayNPCB()
        {
            PCBIDs = new List<string>();
        }
    }
}
