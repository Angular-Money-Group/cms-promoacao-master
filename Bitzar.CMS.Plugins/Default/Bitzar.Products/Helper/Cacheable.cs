using Bitzar.CMS.Extension.CMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bitzar.Products.Helper
{
    public class Cacheable
    {
        public static IDictionaryCache Cache { get; set; } = Plugin.CMS.Cache;
    }
}