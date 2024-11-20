using Opc.Ua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Middleware.Model.AMR
{
    public class Sensor
    {
        public int tm {  get; set; }
        public int infrared { get; set; }
        public int stand {  get; set; }
        public int roller { get; set; }
        public int clip {  get; set; }
        public int clamp { get; set; }
    }
}
