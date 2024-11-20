using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.AspNetCore.Razor.Language.TagHelperMetadata;

namespace Middleware.Model
{
    public class LoaderReport
    {
        public string MagazineId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public List<DateTime> TimeStamp {  get; set; }
        public LoaderReport()
        {
            StartTime = new DateTime(2024, 4, 29);
            EndTime = new DateTime(2024, 4, 30);
            TimeStamp = new List<DateTime>();
        }
    }
}
