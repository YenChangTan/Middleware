using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Middleware.Model
{
    public class MESPost
    {
        public string TaskID { get; set; }
        public string Recipe { get; set; }
        public string PanelBarcode { get; set; }
        public DateTime InTime { get; set; }
        public DateTime OutTime { get; set; }
        public string MachineStatus { get; set; }
        public string AlarmStatus { get; set; }
        public string InspectionStatus { get; set; }
        public string RawData { get; set; }

        public MESPost()
        {
            InTime = new DateTime(2024, 4, 29);
            OutTime = new DateTime(2024, 4, 30);
        }
    }
}
