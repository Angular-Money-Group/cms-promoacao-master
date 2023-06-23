using Bitzar.CMS.Core.Helper;
using Bitzar.CMS.Data.Model;
using System;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Security;

namespace Bitzar.CMS.Core.Areas.admin.Controllers
{
    /// <summary>
    /// Controller to handle all Login and Password options
    /// </summary>
    [RouteArea("Admin", AreaPrefix = "admin")]
    [RoutePrefix("Autenticacao")]
    [HttpsFilter]
    public class AuthenticationController : Controller
    {
        /// <summary>
        /// Method to show login page
        /// </summary>
        /// <param name="ReturnURL">Return URL to continue where user left</param>
        [Route("Login")]
        public ActionResult Login(string ReturnURL)
        {
            ViewBag.ReturnURL = ReturnURL;

#if DEBUG
            ViewBag.DebugUser = "admin";
            ViewBag.DebugPswd = "admin";
#endif

            return View();
        }

        /// <summary>
        /// Method that receives form post action
        /// </summary>
        /// <param name="user">User Name field</param>
        /// <param name="password">Password field</param>
        /// <param name="ReturnURL">Return URL to be redirected</param>
        [HttpPost]
        [Route("Login")]
        public async Task<ActionResult> Login(string user, string password, string ReturnURL)
        {
            try
            {
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

                // Locate user to validate
                using (var db = new DatabaseConnection())
                {
                    var entity = await db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.AdminAccess && !u.Disabled && u.UserName == user);
#if !DEBUG
                    if (entity == null || !Security.Cryptography.Check(password, entity.Password))
                        throw new UnauthorizedAccessException(Resources.Strings.Authentication_UserNotFound);
#endif

                    // Set user session information
                    Session.Set(true, nameof(User.Identity.IsAuthenticated));
                    Session.Set(entity);

                    // Set last access
                    entity.LastLogin = DateTime.Now;
                    await db.SaveChangesAsync();

                    // Set authentication cookie
                    FormsAuthentication.SetAuthCookie(entity.UserName, true);

                    //Trigger Event on login system
                    Functions.CMS.Events.Trigger(Model.Enumerators.EventType.OnLogin, user);

                    // Redirect to the user defined url or not
                    if (!string.IsNullOrWhiteSpace(ReturnURL))
                        return Redirect(ReturnURL);
                    else
                        return Redirect(FormsAuthentication.DefaultUrl);
                }
            }
            catch (Exception ex)
            {
                this.NotifyError(ex, ex.Message);
                return RedirectToAction(nameof(Login), new { ReturnURL });
            }
        }

        /// <summary>
        /// Method to perform Logoff and go to login
        /// </summary>
        /// <param name="ReturnURL">Return URL to continue where user left</param>
        [Route("Logoff")]
        public ActionResult Logoff(string ReturnURL)
        {
            // Perform Logoff operation
            Session.RemoveAll();
            Session.Abandon();
            Session.Clear();

            FormsAuthentication.SignOut();

            // Redirect
            return RedirectToAction(nameof(Login), new { ReturnURL });
        }
    }
}