using System.Collections.Generic;
using System.Globalization;
using Bitzar.CMS.Data.Model;

namespace Bitzar.CMS.Extension.CMS
{
    public interface II18N
    {
        List<Language> AvailableLanguages { get; }
        Language Culture { get; }
        Language DefaultLanguage { get; }

        CultureInfo CultureInfo(string culture);
        string Text(string name, int? culture = null);
    }
}