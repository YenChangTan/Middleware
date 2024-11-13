using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Middleware.Model
{
    public class PCBStatusResult
    {
        public bool HasResult { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
    }
}
