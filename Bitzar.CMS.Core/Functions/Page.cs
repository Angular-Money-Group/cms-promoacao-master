using Bitzar.CMS.Core.Helper;
using Bitzar.CMS.Core.Models;
using Bitzar.CMS.Data.Model;
using Bitzar.CMS.Extension.CMS;
using MethodCache.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bitzar.CMS.Core.Functions.Internal
{
    /// <summary>
    /// Class to hold and organize library functions
    /// </summary>
    [Cache(Members.All)]
    public class Page : Cacheable, IPage
    {
        /// <summary>
        /// Method to load all the available sections in the system
        /// </summary>
        public List<Section> Sections
        {
            get
            {
                using (var db = new DatabaseConnection())
                    return db.Sections.ToList();
            }
        }

        /// <summary>
        /// Method to load all the field types
        /// </summary>
        public List<FieldType> FieldTypes
        {
            get
            {
                using (var db = new DatabaseConnection())
                    return db.FieldTypes.ToList();
            }
        }

        /// <summary>
        /// Property to Allow the User load the current Page in the system
        /// </summary>
        [NoCache]
        public Template Current
        {
            get => (Template)HttpContext.Current.Session["CMS.MAIN.PAGE"];
        }

        /// <summary>
        /// Property to Allow the User load the current Route in the system
        /// </summary>
        [NoCache]
        public RouteParam CurrentRoute
        {
            get => (RouteParam)HttpContext.Current.Session["CMS.MAIN.ROUTE"];
        }

        /// <summary>
        /// Method to return all the available field values to the system
        /// </summary>
        /// <param name="name">Name of the current field</param>
        /// <param name="culture">Specific system culture or default culture of the request</param>
        /// <returns>Returns an object with system data</returns>
        [NoCache]
        public dynamic Field(string name, int row)
        {
            return Field(name, null, row);
        }

        /// <summary>
        /// Method to return all the available field values to the system
        /// </summary>
        /// <param name="name">Name of the current field</param>
        /// <param name="culture">Specific system culture or default culture of the request</param>
        /// <param name="row">Indicates what row should be loaded from database</param>
        /// <returns>Returns an object with system data</returns>
        [NoCache]
        public dynamic Field(string name, int? culture = null, int row = 0)
        {
            // Method to return a field to the system
            var page = Current;
            var lang = culture ?? CMS.I18N.Culture.Id;

            // Locate the field availability for the template
            return CMS.Global.GetField(name, page, lang, row);
        }

        /// <summary>
        /// Method to return all the available field values to the system
        /// </summary>
        /// <param name="fieldType">Type of the current field</param>
        /// <param name="group">Specific the group to get</param>
        /// <returns>Returns an object with system data</returns>
        [NoCache]
        public List<Field> FieldByGroup(int fieldType, string group)
        {

            using (var db = new DatabaseConnection())
            {
                var ticket = db.Fields.Where(s => s.IdFieldType == fieldType && s.Group == group).ToList();
                return ticket;
            }
        }

        /// <summary>
        /// Method to return all the related data from a repeated and their children objects
        /// </summary>
        /// <param name="name">Name of the current repeater</param>
        /// <param name="culture">Specific system culture or default culture of the request</param>
        /// <returns>Returns an object with system data</returns>
        [NoCache]
        public List<IGrouping<int, KeyValuePair<Field, dynamic>>> Repeater(string name, int? culture = null)
        {
            // Method to return a field to the system
            var page = Current;
            var lang = culture ?? CMS.I18N.Culture.Id;

            return CMS.Global.GetRepeater(name, page, lang);
        }

        /// <summary>
        /// Method to return all the available field values to the system
        /// </summary>
        /// <param name="name">Name of the current field</param>
        /// <param name="culture">Specific system culture or default culture of the request</param>
        /// <param name="row">Indicates what row should be loaded from database</param>
        /// <returns>Returns an object with system data</returns>
        [NoCache]
        public dynamic Field(string page, string name, int? culture = null, int row = 0)
        {
            if (string.IsNullOrWhiteSpace(page))
                throw new System.Exception("Page name must be provided");
            if (!page.EndsWith(".cshtml"))
                page += ".cshtml";

            // Method to return a field to the system
            var template = CMS.Functions.Templates.FirstOrDefault(t => t.Name.Equals(page, System.StringComparison.CurrentCultureIgnoreCase))
                ?? throw new System.Exception($"Page {page} was not found in template list.");
            var lang = culture ?? CMS.I18N.Culture.Id;

            // Locate the field availability for the template
            return CMS.Global.GetField(name, template, lang, row);
        }

        /// <summary>
        /// Method to return all the related data from a repeated and their children objects
        /// </summary>
        /// <param name="name">Name of the current repeater</param>
        /// <param name="culture">Specific system culture or default culture of the request</param>
        /// <returns>Returns an object with system data</returns>
        [NoCache]
        public List<IGrouping<int, KeyValuePair<Field, dynamic>>> Repeater(string page, string name, int? culture = null)
        {
            if (string.IsNullOrWhiteSpace(page))
                throw new System.Exception("Page name must be provided");
            if (!page.EndsWith(".cshtml"))
                page += ".cshtml";

            // Method to return a field to the system
            var template = CMS.Functions.Templates.FirstOrDefault(t => t.Name.Equals(page, System.StringComparison.CurrentCultureIgnoreCase))
                ?? throw new System.Exception($"Page {page} was not found in template list.");

            var lang = culture ?? CMS.I18N.Culture.Id;

            return CMS.Global.GetRepeater(name, template, lang);
        }
    }
}