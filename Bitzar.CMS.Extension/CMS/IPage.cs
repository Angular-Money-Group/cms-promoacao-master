using System.Collections.Generic;
using Bitzar.CMS.Core.Models;
using Bitzar.CMS.Data.Model;

namespace Bitzar.CMS.Extension.CMS
{
    public interface IPage
    {
        Template Current { get; }
        RouteParam CurrentRoute { get; }
        List<FieldType> FieldTypes { get; }
        List<Section> Sections { get; }

        dynamic Field(string name, int? culture = null, int row = 0);
        dynamic Field(string name, int row);
        List<System.Linq.IGrouping<int, KeyValuePair<Field, dynamic>>> Repeater(string name, int? culture = null);
    }
}