using Bitzar.CMS.Core.Models;
using Bitzar.CMS.Data.Model;
using Bitzar.CMS.Extension.CMS;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using System.Web;

namespace Bitzar.CMS.Extension.Interfaces
{
    public interface IPlugin
    {
        IList<IMenu> Menus { get; set; }
        IPlugin Setup(ICMS cms);
        dynamic Execute(string function, string token = null, Dictionary<string, string> parameters = null, HttpFileCollectionBase files = null);        
        //dynamic Track(Guid function, Guid plugin);
        IList<INotification> Notifications();
        IList<IMetric> Metrics();
        bool Uninstall();
        IList<IRoute> Routes();
        void ReplicateIdiomKeys();
        void TriggerEvent<T>(string eventType, T data, Exception exception = null);
    }
}