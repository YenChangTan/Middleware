using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Middleware.Model.AMR
{
    public class CreateTaskReturn
    {
        public int status {  get; set; }
        public ReportTaskStatus content { get; set; }
        public int total { get; set; }
        public int timestamp { get; set; }
        public string message { get; set; }
        public int code { get; set; }
    }
}
