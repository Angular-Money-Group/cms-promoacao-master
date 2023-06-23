using Bitzar.CMS.Extension.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitzar.CMS.Extension.Classes
{
    public class Metric : IMetric
    {
        public string Title { get; set; }
        public string Value { get; set; }
        public string Color { get; set; }
        public string Page { get; set; }
    }
}
