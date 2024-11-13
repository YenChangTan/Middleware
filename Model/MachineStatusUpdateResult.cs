using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Middleware.Model
{
    public class MachineStatusUpdateResult
    {
        public bool HasResult { get; set; }
        public string Message { get; set; }
        public string EchoText { get; set; }
    }
}
