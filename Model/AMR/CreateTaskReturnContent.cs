using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Middleware.Model.AMR
{
    public class CreateTaskReturnContent
    {
        public string orderId {  get; set; }
        public string taskId { get; set; }
        public string taskName { get; set; }
        public string carDeviceId {  get; set; }
        public string carName { get; set; }
        public int taskStatus { get; set; }
        public List<Subtask> subtasks { get; set; }
        public int total {  get; set; }
        public int timestamp { get; set; }
        public string message { get; set; }
    }
}
