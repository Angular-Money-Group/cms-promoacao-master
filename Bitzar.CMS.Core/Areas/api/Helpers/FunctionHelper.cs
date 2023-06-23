using Bitzar.CMS.Core.Areas.api.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Bitzar.CMS.Core.Areas.api.Helpers
{
    /// <summary>
    /// Support Helper: Function
    /// </summary>
    public static class FunctionHelper
    {
        private static readonly Functions.Internal.Functions functions = Functions.CMS.Functions;

        /// <summary>
        /// List Templates
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<dynamic> ListTemplates()
        {
            var typeFilter = new[] { "View" };

            return functions.Templates
                .Where(x => typeFilter.Contains(x.TemplateType.Name))
                .Select(s => new
                {
                    s.Id,
                    Name = s.Name.Replace($".{s.Extension}", string.Empty),
                    s.Description,
                    s.Restricted,
                    RoleRestriction = s.RoleRestriction?.Split(',').ToArray(),
                    s.Url,
                    SiteUrl = Functions.CMS.Functions.BaseUrl() + Functions.CMS.Functions.Url(s.Name),
                    s.User,
                    s.Version
                });
        }

        /// <summary>
        /// Get transaction code for template
        /// </summary>
        /// <param name="idTemplate"></param>
        /// <param name="userIsAuthenticated"></param>
        /// <param name="userRoleId"></param>
        /// <returns></returns>
        public static HttpStatusCode GetTemplateTransactionCode(int idTemplate, bool userIsAuthenticated, string userRoleId)
        {
            var template = functions.Templates
                                    .Select(t => new TemplateFieldModel
                                    {
                                        Id = t.Id,
                                        Restricted = t.Restricted,
                                        RoleRestriction = !string.IsNullOrEmpty(t.RoleRestriction) ? t.RoleRestriction?.Split(',').ToList() : new List<string>()
                                    })
                                    .FirstOrDefault(t => t.Id == idTemplate) 
                                        ?? throw new System.Exception(Resources.Strings.Template_NotFound);

            if (template.Restricted && ((!userIsAuthenticated) || (template.RoleRestriction?.Count > 0 && !template.RoleRestriction.Contains(userRoleId))))
                return HttpStatusCode.Unauthorized;

            return HttpStatusCode.OK;
        }
    }
}