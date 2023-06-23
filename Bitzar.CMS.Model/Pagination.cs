using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bitzar.CMS.Core.Models
{
    public class Pagination
    {
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int Size { get; set; }
        public bool ShowNext { get; set; } = true;
        public bool ShowPrevious { get; set; } = true;
        public int MaxPageItems { get; set; } = 5;
    }
}