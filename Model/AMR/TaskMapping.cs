using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Middleware.Model.AMR
{
    public class TaskMapping
    {
        public string name {  get; set; }
        public string deviceId { get; set; }
        public string orderId { get; set; }
        public List<SubTaskInfo> subtaskInfos { get; set; }
        public string thingsId { get; set; }
        
    }

    
}
