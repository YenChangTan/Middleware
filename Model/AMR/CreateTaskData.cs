using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Middleware.Model.AMR
{
    public class CreateTaskData
    {
        public string name { get; set; }
        public string deviceId { get; set; }
        public string type { get; set; }
        public string orderId { get; set; }
        public int sort {  get; set; }
        public List<TaskInfo> task {  get; set; }

    }
}
