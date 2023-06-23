using Bitzar.CMS.Extension.Classes;
using System.Collections.Generic;
using System.Linq;

namespace Bitzar.CMS.Core.Helper
{
    /// <summary>
    /// Internal class to be inherit and expose Cache object
    /// </summary>
    public class Cacheable
    {
        /// <summary>
        /// Global Cache Reference
        /// </summary>
        public static DictionaryCache Cache { get; set; } = Functions.CMS.Cache;

    }
}