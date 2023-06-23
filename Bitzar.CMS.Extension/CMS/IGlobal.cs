using Bitzar.CMS.Data.Model;

namespace Bitzar.CMS.Extension.CMS
{
    public interface IGlobal
    {
        dynamic Field(string name, int? culture = null, int row = 0);
        System.Collections.Generic.List<System.Linq.IGrouping<int, System.Collections.Generic.KeyValuePair<Field, dynamic>>> Repeater(string name, int? culture = null);
    }
}