using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitzar.CMS.Extension.Interfaces
{
    public interface IMetric
    {
        string Title { get; set; }
        string Value { get; set; }
        string Color { get; set; }
        string Page { get; set; }
    }
}
