using Bitzar.CMS.Extension.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Bitzar.CMS.Extension.Classes
{
    public class PluginInfo
    {
        public string Name { get; set; }
        public Version Version { get; set; }
        public FileInfo FileInfo { get; set; }
        public IPlugin Plugin { get; set; }
        public Assembly Assembly { get; set; }
        public AppDomain AppDomain { get; set; }
        public bool Loaded { get; set; } = true;
    }
}
