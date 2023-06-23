using Bitzar.CMS.Core.Models;
using System;
using System.Linq;

namespace Bitzar.CMS.Core.Areas.api.Helpers
{
    /// <summary>
    /// Support Helper: Blog
    /// </summary>
    public static class BlogHelper
    {
        private static readonly Functions.Internal.Blog blog = Functions.CMS.Blog;

        /// <summary>
        /// List Posts
        /// </summary>
        /// <returns></returns>
        public static dynamic Posts(int page = 1, int size = 10, string category = null)
        {
            var response = blog.Navigate(page, size, categories: category);
            var records = response.Records.Select(s => (dynamic)new
            {
                s.Id,
                s.Title,
                s.Categories,
                Subtitle = ConvertToString(s.Subtitle),
                s.Image,
                s.Url,
                s.CreatedAt,
                s.Author,
                s.Media,
                s.Name,
                s.IsFixed,
                s.IsReleased,
                s.Tags,
                PostContent = ConvertToString(s.PostContent)
            }).ToList();

            return new PaggedResult<dynamic>()
            {
                Count = response.Count,
                CountPage = response.CountPage,
                Page = response.Page,
                Size = response.Size,
                Records = records
            };
        }

        /// <summary>
        /// List Categories
        /// </summary>
        /// <returns></returns>
        public static dynamic Categories()
            => blog.Categories;

        private static string ConvertToString(dynamic obj)
            => Convert.ToString(obj);
    }
}