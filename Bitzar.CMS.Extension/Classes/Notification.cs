using Bitzar.CMS.Extension.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitzar.CMS.Extension.Classes
{
    public class Notification : INotification
    {
        public string Title { get; set; }
        public int Badge { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string UrlFunction { get; set; }
        public string Plugin { get; set; }
    }
}
