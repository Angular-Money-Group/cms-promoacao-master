using Bitzar.CMS.Data.Model;
using Bitzar.CMS.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bitzar.CMS.Core.Models
{
    public class RouteParam
    {
        public string Language { get; set; }
        public Language Culture { get; set; }
        public string Section { get; set; }
        public string PageUrl { get; set; }
        public string[] QueryString { get; set; }
        public string[] Parameters { get; set; }
        public Template Page { get; set; }
        public Template BlogPage { get; set; }
        public string Plugin { get; set; }
        public string RouteId { get; set; }
        public string RouteType { get; set; }
        public SearchResult Search { get; set; }
        public DateTime? LastModified { get; set; }

        /// <summary>
        /// Property to replicate the Full Route to compare with user browser input
        /// </summary>
        public string Route
        {
            get
            {
                // Create route pattern
                var route = $"/{Language}/{Section}/{PageUrl}/";

                // Remove duplicated slashs
                while (route.IndexOf("//") >= 0)
                    route = route.Replace("//", "/");

                // Get it back
                return route;
            }
        }

        public override string ToString()
        {
            return this.Route;
        }
    }
}