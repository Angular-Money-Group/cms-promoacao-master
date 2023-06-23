using Bitzar.CMS.Core.Functions.Internal;
using System.Collections.Generic;
using System.Linq;

namespace Bitzar.CMS.Core.Areas.api.Helpers
{
    /// <summary>
    /// Support Helper: Language
    /// </summary>
    public static class LanguageHelper
    {
        private static readonly I18N language = Functions.CMS.I18N;

        /// <summary>
        /// List available languages
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<dynamic> Available()
            => language.AvailableLanguages
                .Select(s => new
                {
                    s.Id,
                    s.Culture,
                    s.Description,
                    s.DateTimeFormat,
                    s.NumberFormat,
                    s.CurrencyFormat,
                    s.DateFormat,
                    s.TimeFormat
                });

        /// <summary>
        /// Get default language
        /// </summary>
        /// <returns></returns>
        public static dynamic Default()
            => language.AvailableLanguages
                .Select(s => new
                {
                    s.Id,
                    s.Culture,
                    s.Description,
                    s.DateTimeFormat,
                    s.NumberFormat,
                    s.CurrencyFormat,
                    s.DateFormat,
                    s.TimeFormat
                })
                .FirstOrDefault(s => s.Culture == Functions.CMS.Configuration.DefaultLanguage);
    }
}