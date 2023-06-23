using System.Collections.Generic;
using System.Linq;

namespace Bitzar.CMS.Core.Areas.api.Helpers
{
    /// <summary>
    /// Support Helper: Text
    /// </summary>
    public static class TextHelper
    {
        private static readonly Functions.Internal.Global global = Functions.CMS.Global;
        private static readonly Functions.Internal.I18N language = Functions.CMS.I18N;

        /// <summary>
        /// List field values available
        /// </summary>
        /// <param name="lang"></param>
        /// <returns></returns>
        public static IEnumerable<dynamic> Text(string lang)
        {
            var idLang = language.AvailableLanguages.FirstOrDefault(l => l.Culture == lang)?.Id ?? language.DefaultLanguage.Id;

            return global.Values
                .Where(x => x.Field.IdTemplate == null && x.IdLanguage == idLang && x.Field.IdParent == null && x.Field.Resource)
                .Select(v => new
                {
                    v.Field.Id,
                    v.Field.Name,
                    v.Field.Group,
                    v.Value
                });
        }
    }
}