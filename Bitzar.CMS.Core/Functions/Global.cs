using Bitzar.CMS.Core.Helper;
using Bitzar.CMS.Data.Model;
using Bitzar.CMS.Extension.CMS;
using MethodCache.Attributes;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Bitzar.CMS.Core.Functions.Internal
{
    /// <summary>
    /// Class to hold and organize library functions
    /// </summary>
    [Cache(Members.All)]
    public class Global : Cacheable, IGlobal
    {
        #region Internal methods to help class work properly
        /// <summary>
        /// Internal property to cache all the field values available
        /// </summary>
        [Cache]
        internal List<FieldValue> Values
        {
            get
            {
                using (var db = new DatabaseConnection())
                    return db.FieldValues
                                .Include(f => f.Field)
                                .Include(f => f.Field.FieldType)
                                .Include(f => f.Field.Children)
                                .Include(f => f.Field.Children.Select(c => c.FieldValues))
                                .Include(f => f.Language).ToList();
            }
        }

        /// <summary>
        /// Internal property to cache all the field types available
        /// </summary>
        internal List<FieldType> FieldTypes
        {
            get
            {
                using (var db = new DatabaseConnection())
                    return db.FieldTypes.ToList();
            }
        }

        /// <summary>
        /// Internal method to allow locate an repeater in the system
        /// </summary>
        /// <param name="name">Field identification name</param>
        /// <param name="page">Related page to locate the field. To Global, set null</param>
        /// <param name="lang">Language identification to load data</param>
        /// <returns></returns>
        internal List<IGrouping<int, KeyValuePair<Field, dynamic>>> GetRepeater(string name, Template page, int lang)
        {
            // Locate the field availability for the template
            var repeater = (from v in CMS.Global.Values
                            where v.Field.IdTemplate == (page?.Id ?? null) &&
                                    v.Field.Name.Trim().Equals(name.Trim(), StringComparison.CurrentCultureIgnoreCase)
                            select v.Field).FirstOrDefault();

            // Get field Children
            var value = (repeater?.Children.SelectMany(c => c.FieldValues).Where(v => v.IdLanguage == lang) ?? new List<FieldValue>());


            // Create key value information and return information
            return (from v in value
                    group (new KeyValuePair<Field, dynamic>(v.Field, GetField(v.Field.Name, page, lang, v.Order))) by v.Order into g
                    orderby g.Key
                    select g).ToList();
        }

        /// <summary>
        /// Internal méthod to allow get field in the system
        /// </summary>
        /// <param name="name">Field identification name</param>
        /// <param name="page">Related page to locate the field. To Global, set null</param>
        /// <param name="lang">Language identification to load data</param>
        /// <param name="row">Row sequence to get data inside Repeater</param>
        /// <returns></returns>
        internal dynamic GetField(string name, Template page, int lang, int row)
        {
            // Locate the field availability for the template
            var value = (from v in CMS.Global.Values
                         where v.Field.IdTemplate == (page?.Id ?? null) &&
                               !v.Field.Resource &&
                               v.Field.Name.Trim().Equals(name.Trim(), StringComparison.CurrentCultureIgnoreCase) &&
                               v.IdLanguage == lang &&
                               v.Order == row
                         select v).FirstOrDefault();

            if (value == null)
                return null;

            // Cast each object type to return proper object instance
            return GetFieldObject(value);
        }

        internal dynamic GetFieldObject(FieldValue value)
        {
            // Cast each object type to return proper object instance
            switch (value.Field.FieldType.Name)
            {
                case "Checkbox":
                    return (value.Value != "false");
                case "Imagem":
                case "Midia":
                    if (string.IsNullOrWhiteSpace(value.Value))
                        return new Data.Model.Library();

                    if (!int.TryParse(value.Value, out var libraryId))
                        return new Data.Model.Library();

                    return CMS.Library.Object(libraryId);
                case "Html":
                    if (string.IsNullOrWhiteSpace(value.Value))
                        return new HtmlString("");

                    return new HtmlString(HttpUtility.HtmlDecode(value.Value));
                case "Galeria":
                    if (string.IsNullOrWhiteSpace(value.Value))
                        return new Data.Model.Library[] { };

                    return (value.Value.Split(',').Select(v => CMS.Library.Object(Convert.ToInt32(v)))).ToList();
                case "Texto":
                    if (string.IsNullOrWhiteSpace(value.Value))
                        return new HtmlString("");

                    return new HtmlString(value.Value.Replace("&break;", $"<span class=\"break\"><br class=\"break field-{value.IdField}\"/></span>").Replace("&break-mobile;", $"<span class=\"break-mobile\"><br class=\"break-mobile field-{value.IdField}\"/></span>"));
                default:
                    return value.Value;
            }
        }
        #endregion

        /// <summary>
        /// Method to return all the available field values to the system
        /// </summary>
        /// <param name="name">Name of the current field</param>
        /// <param name="culture">Specific system culture or default culture of the request</param>
        /// <param name="row">Specific system culture or default culture of the request</param>
        /// <returns>Returns an object with system data</returns>
        [NoCache]
        public dynamic Field(string name, int? culture = null, int row = 0)
        {
            // Method to return a field to the system
            var lang = culture ?? CMS.I18N.Culture.Id;
            return GetField(name, null, lang, row);
        }

        /// <summary>
        /// Method to return all the available field values to the system
        /// </summary>
        /// <param name="name">Name of the current field</param>
        /// <returns>Returns an object with system data</returns>
        [NoCache]
        public dynamic Field(string name, int row)
        {
            // Method to return a field to the system
            var lang = CMS.I18N.Culture.Id;
            return GetField(name, null, lang, row);
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
            var lang = culture ?? CMS.I18N.Culture.Id;
            return GetRepeater(name, null, lang);
        }
    }
}