using System;
using System.Collections.Generic;
using Bitzar.CMS.Core.Models;
using Bitzar.CMS.Data.Model;

namespace Bitzar.CMS.Extension.CMS
{
    public interface IBlog
    {
        IList<string> Categories { get; }
        BlogPost Current { get; }
        IList<RouteParam> Posts { get; }

        IList<BlogPost> MostReaded(int size = 5, DateTime? start = null, string categories = null);
        PaggedResult<BlogPost> Navigate(int page = 1, int size = 10, string filter = "", string categories = "", string tags = "");

        Template CreateBlogPost(Data.Model.User user, string category = null);
    }
}