using Bitzar.CMS.Data.Model;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bitzar.CMS.Core.Areas.api.Helpers
{
    /// <summary>
    /// Support Helper: Global
    /// </summary>
    public static class GlobalHelper
    {
        private static readonly Functions.Internal.Global global = Functions.CMS.Global;
        private static readonly Functions.Internal.I18N language = Functions.CMS.I18N;

        /// <summary>
        /// List Field Types
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<dynamic> ListTypes()
            => global.FieldTypes.Select(x => new { x.Id, x.Name });

        /// <summary>
        /// List Field Values
        /// </summary>
        /// <param name="lang"></param>
        /// <param name="idTemplate"></param>
        /// <returns></returns>
        public static IEnumerable<dynamic> ListValues(string lang, int? idTemplate = null)
        {
            var idLang = language.AvailableLanguages.FirstOrDefault(l => l.Culture == lang)?.Id ?? language.DefaultLanguage.Id;

            return global.Values
                .Where(x => x.Field.IdTemplate == idTemplate && x.IdLanguage == idLang && x.Field.IdParent == null && !x.Field.Resource)
                .Select(v => new
                {
                    v.Field.Id,
                    v.Field.Description,
                    v.Field.Group,
                    v.Field.Name,
                    v.Field.SelectData,
                    FieldType = v.Field.FieldType.Name,
                    Value = ConvertMedia(v),
                    v.Order,
                    Children = v.Field.Children.SelectMany(c => c.FieldValues).Where(c => c.IdLanguage == idLang).Select(x => new
                    {
                        x.Field.Id,
                        x.Field.Description,
                        x.Field.Group,
                        x.Field.Name,
                        x.Field.SelectData,
                        FieldType = x.Field.FieldType.Name,
                        Value = ConvertMedia(x),
                        x.Order
                    })
                    .GroupBy(z => z.Order)
                    .OrderBy(z => z.Key)
                    .Select(z => new { Record = z.Key, Values = z.ToList() })
                })
                .GroupBy(x => x.Group)
                .Select(x => new { Group = x.Key, Items = x.ToList() });
        }

        /// <summary>
        /// Support private method to convert media data
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static dynamic ConvertMedia(FieldValue value)
        {
            var result = global.GetFieldObject(value);
            if (result is HtmlString resultHtmlString)
                return resultHtmlString.ToHtmlString();

            return result;
        }
    }
}