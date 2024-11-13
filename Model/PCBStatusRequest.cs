﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Middleware.Model
{
    public class PCBStatusRequest
    {
        public string TaskName { get; set; }
        public string Barcode {  get; set; }

        public PCBStatusRequest()
        {
            TaskName = "PCB Status";
        }
    }
}