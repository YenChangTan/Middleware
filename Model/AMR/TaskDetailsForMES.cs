using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Middleware.Model.AMR
{
    public class TaskDetailsForMES
    {
        public string orderId { get; set; }
        public string taskId { get; set; }
        public bool isDone { get; set; }
        public TaskDetailsForMES()
        {
            isDone = false;
        }
    }
}
