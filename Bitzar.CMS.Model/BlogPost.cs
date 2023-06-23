using Bitzar.CMS.Data.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bitzar.CMS.Core.Models
{
    public class BlogPost
    {
        /// <summary>
        /// Page reference to be stored and get information
        /// </summary>
        [JsonIgnore]
        public Template Page { get; set; }

        /// <summary>
        /// Internal members to store information from values used when rebuild index
        /// </summary>
        [JsonIgnore]
        public dynamic image;
        [JsonIgnore]
        public dynamic subtitle;
        [JsonIgnore]
        public dynamic postContent;
        [JsonIgnore]
        public dynamic categories;
        [JsonIgnore]
        public dynamic tags;
        [JsonIgnore]
        public dynamic isFixed;
        [JsonIgnore]
        public dynamic media;
        [JsonIgnore]
        public dynamic title;

        public BlogPost(Template page)
        {
            this.Page = page;
            this.title = page.Description;
        }

        public int Id => Page.Id;
        public string Name => Page.Name;
        public string Title => title != null ? title.ToString() : "";
        public string Url => Page.Url;
        public dynamic Image => image;
        public dynamic Subtitle => subtitle;
        public dynamic PostContent => postContent;
        public string[] Categories => categories;
        public string[] Tags => tags;
        public bool IsFixed => isFixed;
        public bool IsReleased => Page.Released;
        public DateTime CreatedAt => Page.CreatedAt;
        public string Author => Page.User;
        public dynamic Media => media;
        public ICollection<Field> Fields { get => Page.Fields; }

        public Field this[string key] => this.Fields.FirstOrDefault(f => f.Name == key);
    }
}