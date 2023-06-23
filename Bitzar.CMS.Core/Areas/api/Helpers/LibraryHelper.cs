using System.Collections.Generic;
using System.Linq;

namespace Bitzar.CMS.Core.Areas.api.Helpers
{
    /// <summary>
    /// Support Helper: Library
    /// </summary>
    public static class LibraryHelper
    {
        private static readonly Functions.Internal.Library library = Functions.CMS.Library;

        /// <summary>
        /// List library types
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<dynamic> Types()
            => library.Types().Select(x => new
            {
                x.Id,
                x.Description,
                x.MimeTypes,
                x.AllowedExtensions
            });

        /// <summary>
        /// List library objects
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<dynamic> Objects() 
            => library.Objects().Select(x => new
            {
                x.Id,
                x.Name,
                x.Description,
                x.Extension,
                x.Size,
                x.CreatedAt,
                x.Attributes,
                x.SizeInKB,
                x.SizeInMB,
                Url = x.FullPath,
                x.IdLibraryType
            });
    }
}