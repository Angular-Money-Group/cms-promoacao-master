using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitzar.CMS.Extension.Interfaces
{
    public interface IMenu
    {
        string Name { get; set; }
        IList<IMenuItem> Items { get; set; }
    }
}
