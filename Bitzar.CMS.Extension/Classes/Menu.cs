using Bitzar.CMS.Extension.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitzar.CMS.Extension.Classes
{
    public class Menu : IMenu
    {
        public string Name { get; set; }
        public IList<IMenuItem> Items { get; set; } = new List<IMenuItem>();
    }
}
