using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bitzar.CMS.Core.Areas.admin.Helpers
{
    public class Icon
    {
        public static string GetIcon(string extension)
        {
            var specificType = "";
            var icon = "";
            try
            {
                var types = Functions.CMS.Library.Types().Select(x => new { x.AllowedExtensions, x.Description }).ToList();

                foreach(var type in types)
                {
                    var splitedTypes = type.AllowedExtensions.Split(',').ToList();

                    if (splitedTypes.Any(p => p == extension))
                        specificType = type.Description;
                }

                switch(specificType)
                {
                    case "Image":
                        icon = "wb-image";
                        break;
                    case "Audio":
                        icon = "wb-musical";
                        break;
                    case "Video":
                        icon = "wb-video";
                        break;
                    case "Other":
                        icon = "wb-attach-file";
                        break;
                    default:
                        icon = "";
                        break;
                }

            }
            catch(Exception ex)
            {
                throw ex;
            }
            return icon;
        }
    }
}