using Bitzar.CMS.Core.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Bitzar.CMS.Core.Controllers
{
    public class MainController : MainBaseController
    {
        /// <summary>
        /// Default page Render system to the CMS
        /// </summary>
        /// <returns></returns>
        [RunSetup(Order = 0), DefaultUrlFilter(Order = 1), Authenticate(Order = 2), Statistic(Order = 3)]
        public ActionResult PageRenderer(/*string lang, string section, string id*/)
        {
            try
            {
                // Split system segments and get current Route
                var route = Functions.CMS.Functions.MatchRoute(Request.Url);
                if (route == null)
                {
                    var page404 = Functions.CMS.Functions.Templates.FirstOrDefault(x => x.Name == "404.cshtml");
                    Session.Set(page404, "CMS.MAIN.PAGE");
                    return View("404");
                }

                // Set route data on session if desired to access
                Session.Set(route.Culture, "CMS.MAIN.CULTURE");
                Session.Set(route, "CMS.MAIN.ROUTE");

                // if request refers to a blog post, return the blog page holder
                if (route.Page.IsBlogPost())
                {
                    Session.Set(route.BlogPage, "CMS.MAIN.PAGE");
                    return View(route.BlogPage.Name.Replace($".{route.Page.Extension}", ""));
                }
                else
                    // Store page on session
                    Session.Set(route.Page, "CMS.MAIN.PAGE");

                // Return requested page
                return View(route.Page.Name.Replace($".{route.Page.Extension}", ""));
            }
            catch (Exception ex)
            {
                var parameters = new
                {
                    Exception = ex,
                    Controller = this.Request.RequestContext.RouteData.DataTokens["controller"]?.ToString(),
                    Action = this.Request.RequestContext.RouteData.DataTokens["action"]?.ToString(),
                    Url = this.Request.Url.ToString()
                };
                Functions.CMS.Log.LogRequest(parameters);

                return View("500");
            }
        }

        /// <summary>
        /// Ajax processor to process functions and return information to the system
        /// </summary>
        /// <param name="function">Function name to be executed</param>
        /// <param name="output">Output type of Result. Allowed JSON or VIEW name.</param>
        /// <param name="parameters">Array of parameters name to bind in the system</param>
        /// <param name="values">Array of parameters values to bind in the system</param>
        /// <returns></returns>
        public ActionResult Ajax(string function = "", string output = "", string[] parameters = null, string[] values = null)
        {
            try
            {
                // Validate if the output requested exists in the system
                if (!output.Equals("JSON", StringComparison.CurrentCultureIgnoreCase))
                    if (!Functions.CMS.Functions.Templates.Any(t => t.TemplateType.Name == "Partial" && Path.GetFileNameWithoutExtension(t.Name).Equals(output, StringComparison.CurrentCultureIgnoreCase)))
                        throw new Exception("Output type not found.");

                // Check if the number of parameters are equals to the number of values
                if ((parameters?.Length ?? 0) != (values?.Length ?? 0))
                    throw new Exception("Parameters names and values does not match the same size.");

                // Create the parameter list
                var parameter = new Dictionary<string, string>();
                for (var i = 0; i < (parameters?.Length ?? 0); i++)
                    parameter.Add(parameters[i], values[i]);

                dynamic result;
                switch (function)
                {
                    case "CMS.Blog.Navigate":
                        {
                            // Try to get all the parameters provided
                            if (!parameter.TryGetValue("page", out string page)) page = "1";
                            if (!parameter.TryGetValue("size", out string size)) size = "10";
                            parameter.TryGetValue("filter", out string filter);
                            parameter.TryGetValue("categories", out string categories);
                            parameter.TryGetValue("tags", out string tags);

                            // Execute routine
                            result = Functions.CMS.Blog.Navigate(Convert.ToInt32(page), Convert.ToInt32(size), filter, categories, tags);
                        }
                        break;
                    case "CMS.Blog.Filter":
                        {
                            if (!parameter.TryGetValue("page", out string page)) page = "1";
                            if (!parameter.TryGetValue("size", out string size)) size = "10";

                            var filter = parameter.Where(x => x.Key != "page" && x.Key != "size").ToDictionary(x => x.Key, x => x.Value);

                            result = Functions.CMS.Blog.Filter(Convert.ToInt32(page), Convert.ToInt32(size), filter);
                        }
                        break;
                    default:
                        throw new Exception("Function not allowed.");
                }

                // Handle result return type
                if (output.Equals("JSON", StringComparison.CurrentCultureIgnoreCase))
                    return Json(result, JsonRequestBehavior.AllowGet);

                // If partial view result, store data in the ViewBag.AjaxModel
                ViewBag.AjaxParameters = parameter;
                ViewBag.AjaxModel = result;
                return PartialView(output);
            }
            catch (Exception ex)
            {
                var parameter = new
                {
                    Exception = ex,
                    Controller = this.Request.RequestContext.RouteData.Values["controller"]?.ToString(),
                    Action = this.Request.RequestContext.RouteData.Values["action"]?.ToString(),
                    Url = this.Request.Url,
                    Function = function,
                    Parameters = parameters
                };

                Functions.CMS.Log.LogRequest(parameter);
                return Json(new { error = ex.AllMessages() }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Method to get response information and send to the "source" to be executed.
        /// Any handling of the process is designed to be processed by the Plugin that will be selected
        /// by the source provider;
        /// </summary>
        /// <param name="source">Plugin name to match the name of the target</param>
        /// <param name="function">Function to be processed inside the plugin</param>
        /// <param name="token">Token to autenticate the request in the plugin</param>
        /// <param name="output">Output format to return data to the system</param>
        /// <param name="parameters">List of parameters send</param>
        /// <param name="values">List of value to match the parameter names</param>
        /// <param name="template">Define the template to be used when output is MAIL</param>
        /// <param name="subject">Define the subject to be used when output is MAIL</param>
        /// <param name="mailTo">Define the emails to send</param>
        /// <param name="mailBcc">Define the blinded emails to send</param>
        /// <returns>Returns anything or nothing that comes from the plugin.</returns>
        [HttpPost, ValidateInput(false)]
        [Throttling(TimeUnit.Instantly), Throttling(TimeUnit.Minutely), Throttling(TimeUnit.Hourly), Throttling(TimeUnit.Daily)]
        public async Task<ActionResult> Execute(string source, string function, string token = null, string output = "", string[] parameters = null, string[] values = null, string template = null, string subject = null, string mailTo = null, string mailBcc = null)

        {

            // Validate if the function is a partial to be rendered
            if (function.ToLower() == "partial")
                return PartialView(output);

            // Validation of the system is find the right plugin to process the request
            var plugin = Functions.CMS.Plugins.Available.FirstOrDefault(p => p.Name.Equals(source, StringComparison.CurrentCultureIgnoreCase));
            if (plugin == null)
                return Json(new { error = Resources.Strings.Plugins_NotFound }, JsonRequestBehavior.AllowGet);

            // Check if the provided parameter are equal
            if ((parameters?.Length ?? 0) != (values?.Length ?? 0))
                throw new Exception("Parameters names and values does not match the same size.");

            // Create the parameter list to be stored in the ViewBag
            var paramList = new Dictionary<string, string>();
            for (var i = 0; i < (parameters?.Length ?? 0); i++)
                paramList.Add(parameters[i], values[i]);

            // add in paramList the other parameters that came in Request.Form
            foreach (var key in Request.Unvalidated.Form.AllKeys)
                if (!paramList.ContainsKey(key))
                    paramList.Add(key, Request.Unvalidated.Form[key]);

            try
            {
                // PreValidate in catch all validation plugin
                Functions.CMS.Events.Trigger(Model.Enumerators.EventType.PreValidateExecute, paramList);

                // Call the method to process the request inside the Plugin
                var result = plugin.Plugin.Execute(function, token, paramList, Request.Files);

                // Threat result to response as json or refresh the call page
                if (output.Equals("JSON", StringComparison.CurrentCultureIgnoreCase))
                    return Json(result, JsonRequestBehavior.AllowGet);

                // Threat result to response as xml or refresh the call page
                if (output.Equals("XML", StringComparison.CurrentCultureIgnoreCase))
                    return new XmlActionResult(result);

                // Return Download if available
                if (output.Equals("DOWNLOAD", StringComparison.CurrentCultureIgnoreCase) && (result is FileStreamResult))
                    return (result as FileStreamResult);

                // Send output type to mail
                if (output.Equals("MAIL", StringComparison.CurrentCultureIgnoreCase))
                {
                    // Validate if the fields are valid
                    if (string.IsNullOrWhiteSpace(template) || string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(mailTo))
                        throw new Exception("Parameters Template, Subject and MailTo are required when Output is MAIL.");

                    // Split mailto list to send
                    var mailToList = mailTo.Split(';').Select(m => m.Trim()).ToArray();
                    var mailBccToList = new string[] { };

                    //Validate if the field is not empty
                    if (!string.IsNullOrWhiteSpace(mailBcc))
                        mailBccToList = mailBcc.Split(';').Select(m => m.Trim()).ToArray();

                    // Check if there is any mail restriction
                    var mailRestriction = Functions.CMS.Configuration.Get("AllowedEmails");
                    if (!string.IsNullOrWhiteSpace(mailRestriction))
                    {
                        // Check if the target is allowed
                        var restrictionList = mailRestriction.Split(';').Select(m => m.Trim());
                        if (!mailToList.All(m => restrictionList.Any(r => r.Equals(m, StringComparison.CurrentCultureIgnoreCase))))
                            throw new Exception(Resources.Strings.MailService_TargetMailNotAllowed);
                    }

                    // Store the model and Trigger mail notification
                    TempData["NotificationData"] = result;

                    await this.TriggerNotification(template, $"{Functions.CMS.Configuration.SiteName} / {subject}", mailToList, mailBcc: mailBccToList, attachments: Request.Files);

                    // Finish the mail send process
                    return Json(new { output, status = "OK" }, JsonRequestBehavior.AllowGet);
                }

                // Store result in ViewBag
                TempData["ExecuteModel"] = result;
                TempData["ExecuteParameters"] = paramList;
            }
            catch (Exception e)
            {
                var stg = new StringBuilder();
                foreach (var i in paramList)
                {
                    stg.AppendLine(i.Key + ":" + i.Value);
                }

                var parameter = new
                {
                    Exception = e,
                    Controller = this.Request.RequestContext.RouteData.Values["controller"]?.ToString(),
                    Action = this.Request.RequestContext.RouteData.Values["action"]?.ToString(),
                    Url = this.Request.Url,
                    Source = source,
                    Function = function,
                    Token = token,
                    Output = output,
                    Parameters = stg,
                    Template = template
                };

                Functions.CMS.Log.LogRequest(parameter);

                if (output.Equals("JSON", StringComparison.CurrentCultureIgnoreCase) ||
                    output.Equals("DOWNLOAD", StringComparison.CurrentCultureIgnoreCase) ||
                    output.Equals("MAIL", StringComparison.CurrentCultureIgnoreCase))
                    return Json(new { error = e.AllMessages() }, JsonRequestBehavior.AllowGet);

                TempData["ExecuteException"] = e;
            }

            // Check if return is partial
            var partialType = Functions.CMS.Functions.TemplateTypes.FirstOrDefault(t => t.Name == "Partial");
            if (Functions.CMS.Functions.Templates.FirstOrDefault(t => t.Name.Equals($"{output}.{partialType.DefaultExtension}", StringComparison.CurrentCultureIgnoreCase))?.IdTemplateType == partialType.Id)
                return PartialView(output);

            // Check if exists a partial view in the plugin handling full path parameters
            if (!output.Contains("/") && !output.Contains("&"))
            {
                var partialPlugin = PartialView($"{plugin.Version}/{source}/{output}");
                if (partialPlugin != null)
                    return partialPlugin;
            }

            // The request was not for JSON result so return to the caller or specific output
            if (string.IsNullOrWhiteSpace(output))
                return Redirect(Request.UrlReferrer.ToString());

            return Redirect(output);
        }
    }
}