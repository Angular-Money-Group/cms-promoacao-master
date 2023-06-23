using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bitzar.CMS.Core.Models
{
    /// <summary>
    /// Class to store a pagination object in the service
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    public class PaggedResult<TModel>
    {
        /// <summary>
        /// Total number of records available in the dataset.
        /// </summary>
        public int Count { get; set; }
        /// <summary>
        /// Page number to be returned
        /// </summary>
        public int Page { get; set; }
        /// <summary>
        /// The size of pagination data
        /// </summary>
        public int Size { get; set; }
        /// <summary>
        /// Total number of pages to be processed
        /// </summary>
        public int CountPage { get; set; }

        /// <summary>
        /// List with the records inside
        /// </summary>
        public IList<TModel> Records { get; set; }

        public static PaggedResult<TModel> Create(IList<TModel> list, int page, int size)
        {
            if (list == null)
                list = new List<TModel>();

            if (size == 0)
                size = 1;
            if (page == 0)
                page = 1;

            return new PaggedResult<TModel>()
            {
                Count = list.Count,
                Page = page,
                Size = size,
                CountPage = Convert.ToInt32(Math.Ceiling((decimal)list.Count / size)),
                Records = list.Skip((page - 1) * size).Take(size).ToList()
            };
        }
    }
}