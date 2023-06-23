using Bitzar.CMS.Core.Helper;
using Bitzar.CMS.Core.Models;
using Bitzar.CMS.Model;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Bitzar.CMS.Core.Controllers
{
    [Statistic]
    [Throttling(TimeUnit.Instantly), Throttling(TimeUnit.Minutely), Throttling(TimeUnit.Hourly), Throttling(TimeUnit.Daily)]
    public class SystemController : Controller
    {
        Functions.Internal.Log log = new Functions.Internal.Log();
        /// <summary>
        /// Method to send notification to someone else
        /// </summary>
        /// <returns></returns>
        [HttpPost, Route("Sistema/Notificacao"), ValidateInput(false)]

        public async Task<JsonResult> Notification(string template, string to, string cc, string bcc, string reply, string subject, UrlFileAttachment[] attachments = null)
         {
            try
            {
                // Handle bypass parameter to fool hackers
                if (!string.IsNullOrWhiteSpace(Request.Form["bypass"]))
                    return Json(new JsonResponse() { Code = System.Net.HttpStatusCode.NoContent, Result = "bypass" }, JsonRequestBehavior.AllowGet);

                // Validate captcha
                if (Functions.CMS.Configuration.EnforceCaptcha)
                {
                    var ip = MvcApplication.GetClientIp();
                    var captcha = Request.Form["g-recaptcha-response"];
                    if (string.IsNullOrWhiteSpace(captcha))
                        throw new Exception(Resources.Strings.Authentication_CaptchaRequired);

                    var captchaResult = Recaptcha.Validate(Functions.CMS.Configuration.Get("CaptchaValidationSite"), Functions.CMS.Configuration.Get("CaptchaSecretKey"), captcha, ip);
                    if (!captchaResult.Succeeded)
                        throw new Exception(Resources.Strings.Authentication_CaptchaRequired);
                }

                // Check if the target was provided
                if (string.IsNullOrWhiteSpace(to))
                    throw new Exception(Resources.Strings.MailService_TargetMailNotDefined);

                var mailToList = to.Split(';').Select(m => m.Trim()).ToArray();
                var mailCcList = cc?.Split(';').Select(m => m.Trim()).ToArray();
                var mailBccList = bcc?.Split(';').Select(m => m.Trim()).ToArray();
                var replyList = reply?.Split(';').Select(m => m.Trim()).ToArray();

                // Check if there is any mail restriction
                var mailRestriction = Functions.CMS.Configuration.Get("AllowedEmails");
                if (!string.IsNullOrWhiteSpace(mailRestriction))
                {
                    // Check if the target is allowed
                    var restrictionList = mailRestriction.Split(';').Select(m => m.Trim());
                    if (!mailToList.All(m => restrictionList.Any(r => r.Equals(m, StringComparison.CurrentCultureIgnoreCase))))
                        throw new Exception(Resources.Strings.MailService_TargetMailNotAllowed);

                    if (mailCcList != null)
                        if (!mailCcList.All(m => restrictionList.Any(r => r.Equals(m, StringComparison.CurrentCultureIgnoreCase))))
                            throw new Exception(Resources.Strings.MailService_TargetMailNotAllowed);

                    if (mailBccList != null)
                        if (!mailBccList.All(m => restrictionList.Any(r => r.Equals(m, StringComparison.CurrentCultureIgnoreCase))))
                            throw new Exception(Resources.Strings.MailService_TargetMailNotAllowed);
                }

                // Check if the template was provided
                if (string.IsNullOrWhiteSpace(template))
                    throw new Exception(Resources.Strings.MailService_TemplateNotificationNotDefined);

                // Create parameter list
                dynamic expando = new ExpandoObject();
                var dictionary = (IDictionary<string, object>)expando;
                foreach (var key in Request.Unvalidated.Form.AllKeys)
                    dictionary.Add(key, Request.Unvalidated.Form[key]);
                TempData["NotificationData"] = expando;

                // Trigger event to record the notification
                Functions.CMS.Events.Trigger(Enumerators.EventType.OnNotification, expando);

                await this.TriggerNotification(template, $"{Functions.CMS.Configuration.SiteName} / {subject}", mailToList, mailCcList, mailBccList, replyList, Request.Files, attachments);
                
                // Return response to the user
                return Json(new JsonResponse() { Result = "OK" }, JsonRequestBehavior.AllowGet);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Json(new JsonResponse() { Code = System.Net.HttpStatusCode.Unauthorized, Error = ex.AllMessages() }, JsonRequestBehavior.AllowGet);
            }
            catch (InvalidOperationException ex)
            {
                return Json(new JsonResponse() { Code = System.Net.HttpStatusCode.NotAcceptable, Error = ex.AllMessages() }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var parameters = new
                {
                    Exception = ex,
                    Controller = this.Request.RequestContext.RouteData.DataTokens["controller"]?.ToString(),
                    Action = this.Request.RequestContext.RouteData.DataTokens["action"]?.ToString(),
                    Url = this.Request.Url.ToString(),
                    Template = template,
                    Email = to + cc + bcc + reply + subject,
                    Attachments = attachments
                };
                log.LogRequest(parameters);
                return Json(new JsonResponse() { Code = System.Net.HttpStatusCode.BadRequest, Error = ex.AllMessages() }, JsonRequestBehavior.AllowGet);
            }
        }

        
    }
}