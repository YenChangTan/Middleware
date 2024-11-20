using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Middleware.Model.AMR
{
    public class TaskInfo
    {
        public string type { get; set; }
        public string name { get; set; }
        public string callMaterialId { get; set; }
        public ThingsId things { get; set; } 
    }
}
