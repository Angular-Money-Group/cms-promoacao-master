using Bitzar.CMS.Core.Helper;
using Bitzar.CMS.Extension.CMS;
using MethodCache.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace Bitzar.CMS.Core.Functions
{
    /// <summary>
    /// Class to mantain the properties that relate to the paths of the system
    /// </summary>
    [Cache(Members.All)]
    public class Path : Cacheable, IPath
    {
        private const string TEMP_PATH = "~/content/temp";

        /// <summary>
        /// Return the temp directory to store anything that should be considered temporary
        /// </summary>
        public string Temp
        {
            get
            {
                var path = HostingEnvironment.MapPath(TEMP_PATH);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                return path;
            }
        }

    }
}