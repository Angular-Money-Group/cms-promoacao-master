using Bitzar.CMS.Core.Helper;
using Bitzar.CMS.Data.Model;
using Bitzar.CMS.Extension.CMS;
using MethodCache.Attributes;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Bitzar.CMS.Core.Functions.Internal
{
    /// <summary>
    /// Class to hold and organize library functions
    /// </summary>
    [Cache(Members.All)]
    public class I18N : Cacheable, II18N
    {
        /// <summary>
        /// Method to Load all the languages available
        /// </summary>
        /// <returns></returns>
        public List<Language> AvailableLanguages
        {
            get
            {
                using (var db = new DatabaseConnection())
                    return db.Languages.ToList();
            }
        }

        /// <summary>
        /// Property to Load the current Available Language Service
        /// </summary>
        /// <returns></returns>
        public Language DefaultLanguage
        {
            get => AvailableLanguages.FirstOrDefault(l => l.Culture == CMS.Configuration.DefaultLanguage);
        }

        /// <summary>
        /// Property to Allow the User load the current culture
        /// </summary>
        [NoCache]
        public Language Culture
        {
            get => (Language)(HttpContext.Current.Session["CMS.MAIN.CULTURE"] ?? CMS.I18N.DefaultLanguage);
        }

        /// <summary>
        /// Method to return the current culture information of the desired language
        /// </summary>
        /// <param name="culture">Culture name to return</param>
        /// <returns>Returns an instance of the Culture Object</returns>
        public CultureInfo CultureInfo(string culture)
        {
            return new CultureInfo(culture);
        }

        /// <summary>
        /// Method to get System Resource defined by the current culture
        /// </summary>
        /// <param name="name">Resource Identification</param>
        /// <param name="culture">Specific culture or nothing</param>
        /// <returns>Returns string data to show on system</returns>
        [NoCache]
        public string Text(string name, int? culture = null)
        {
            var lang = culture ?? CMS.I18N.Culture.Id;
            return GetText(name, lang);
        }

        /// <summary>
        /// Internal cacheble method to get System Resource defined by the current culture
        /// </summary>
        /// <param name="name">Resource Identification</param>
        /// <param name="culture">Specific culture or nothing</param>
        /// <returns>Returns string data to show on system</returns>
        private string GetText(string name, int culture)
        {
            // Locate field value
            var value = (from v in CMS.Global.Values
                         where v.Field.IdTemplate == null &&
                             v.Field.Resource &&
                             v.Field.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase) &&
                             v.IdLanguage == culture
                         select v).FirstOrDefault();

            return value?.Value ?? string.Empty;
        }

        /// <summary>
        /// Method to perform system replication of the values. The system will fill all the translations that are null or blank with the
        /// corresponding value from the default Idiom of the web site.
        /// </summary>
        [NoCache]
        internal async Task ReplicateValues()
        {
            // Get default idiom
            var culture = DefaultLanguage;

            using (var db = new DatabaseConnection())
            {
                DbContextTransaction transaction = null;

                try
                {
                    // Get all the Fields from default language
                    var values = db.FieldValues.AsNoTracking().Where(v => v.IdLanguage == culture.Id).ToList();

                    // Start transaction to avoid any problem
                    transaction = db.Database.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);

                    // Get all the idioms except default
                    foreach (var idiom in AvailableLanguages)
                    {
                        // Skip default
                        if (idiom.Culture == culture.Culture)
                            continue;

                        // Get all fields from the idiom
                        var idiomValues = db.FieldValues.Where(v => v.IdLanguage == idiom.Id).ToList();

                        // Replicate all the translation values
                        foreach (var value in idiomValues)
                        {
                            // Skip if idiom is already filled
                            if (value.Value != null && !string.IsNullOrWhiteSpace(value.Value))
                                continue;

                            // Get current value and check if it's filled
                            var defaultValue = values.FirstOrDefault(v => v.IdField == value.IdField && v.Order == value.Order);
                            if (defaultValue.Value == null || string.IsNullOrWhiteSpace(defaultValue.Value))
                                continue;

                            // Update field
                            value.Value = defaultValue.Value;
                        }
                    }
                    
                    // Apply Changes
                    await db.SaveChangesAsync();
                    transaction.Commit();

                    // Replicate inside plugins
                    foreach (var plugin in CMS.Plugins.Available)
                        plugin.Plugin.ReplicateIdiomKeys();

                    // Clear cache data
                    CMS.ClearCache(typeof(I18N).FullName);
                    CMS.ClearRoutes();
                }
                catch (Exception ex)
                {
                    transaction?.Rollback();
                    throw ex;
                }
            }
        }
    }
}