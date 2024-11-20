using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Middleware.Model.AMR
{
    public class CarList
    {
        public int status {  get; set; }
        public List<Car> content = new List<Car>();
    }
}
