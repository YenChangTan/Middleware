using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Middleware.Model.AMR
{
    public class Car
    {
        public string id {  get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public int error { get; set; }
        public string errorMessage { get; set; }
        public string deviceId { get; set; }
        public int battery {  get; set; }
        public int batteryCharging { get; set; }
        public float speed { get; set; }
        public float rad {  get; set; }
        public TaskInfo doTask { get; set; }
    }
}
