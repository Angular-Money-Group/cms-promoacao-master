using Bitzar.CMS.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

/// <summary>
/// Static class to hold all the methods to extend HtmlHelper main object
/// </summary>
public static class HtmlHelpers
{
    public static HtmlString RenderPagination(this HtmlHelper helper, Pagination pagination, string UrlBase, string pageParameter = "pagina", bool ajax = true)
    {
        // Calc first page number and last page number
        var factor = (int)Math.Floor(pagination.MaxPageItems / 2m);
        var min = (pagination.CurrentPage - factor <= 0 ? 1 : pagination.CurrentPage - factor);
        var max = (pagination.CurrentPage + factor >= pagination.TotalPages ? pagination.TotalPages : pagination.CurrentPage + factor);
        var url = $"{UrlBase}{(UrlBase.IndexOf("?") > 0 ? "&" : "?")}{pageParameter}=";
        var href = $"{(ajax ? "href=\"#\" data-ajax-url" : "href")}";

        // Create string
        var str = new StringBuilder();
        str.AppendLine("<nav>");
        str.AppendLine("    <ul class=\"pagination pagination-sm margin-0\">");
        if (pagination.ShowPrevious)
        {
            var disabled = pagination.CurrentPage == 1;

            str.AppendLine($"        <li{(disabled ? " class=\"disabled\"" : "")}>");
            str.AppendLine($"            <a {href}=\"{(disabled ? "" : $"{url}{pagination.CurrentPage-1}")}\" aria-label=\"Previous\">");
            str.AppendLine("                <span aria-hidden=\"true\">«</span>");
            str.AppendLine("            </a>");
            str.AppendLine("        </li>");
        }

        // Replicate 
        for (var i = min; i <= max; i++)
        {
            // Parameters
            var current = i == pagination.CurrentPage;


            // Create page item
            str.AppendLine($"        <li{(current ? " class=\"active\"" : "")}><a {href}=\"{(current ? "" : url + i)}\">{i}</a></li>");
        }

        if (pagination.ShowNext)
        {
            var disabled = pagination.CurrentPage == pagination.TotalPages;
            str.AppendLine($"        <li{(disabled ? " class=\"disabled\"" : "")}>");
            str.AppendLine($"            <a {href}=\"{(disabled ? "" : $"{url}{pagination.CurrentPage + 1}")}\" aria-label=\"Next\">");
            str.AppendLine("                <span aria-hidden=\"true\">»</span>");
            str.AppendLine("            </a>");
            str.AppendLine("        </li>");
        }
        str.AppendLine("    </ul>");
        str.AppendLine("</nav>");

        // Return value to render
        return new HtmlString(str.ToString());
    }
}