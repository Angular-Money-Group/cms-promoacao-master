using Bitzar.CMS.Core.Helper;
using Bitzar.CMS.Core.Models;
using Bitzar.CMS.Data.Model;
using System;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.WebPages;

namespace Bitzar.CMS.Core.Controllers
{
    [Statistic]
    [Throttling(TimeUnit.Instantly), Throttling(TimeUnit.Minutely), Throttling(TimeUnit.Hourly), Throttling(TimeUnit.Daily)]
    public class AuthenticationController : Controller
    {
        Functions.Internal.Log log = new Functions.Internal.Log();
        /// <summary>
        /// Method to authenticate membership
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        [HttpPost, Route("Autenticacao/Login")]
        public async Task<JsonResult> Login(string username, string password)
        {
            try
            {
                // Handle bypass parameter to fool hackers
                if (!string.IsNullOrWhiteSpace(Request.Form["bypass"]))
                    return Json(new JsonResponse() { Code = System.Net.HttpStatusCode.NoContent, Result = "bypass" }, JsonRequestBehavior.AllowGet);

                // Validate captcha
                if (Functions.CMS.Configuration.EnforceCaptcha && !string.IsNullOrWhiteSpace(password))
                {
                    var ip = MvcApplication.GetClientIp();
                    var captcha = Request.Form["g-recaptcha-response"];
                    if (string.IsNullOrWhiteSpace(captcha))
                        throw new Exception(Resources.Strings.Authentication_CaptchaRequired);

                    var captchaResult = Recaptcha.Validate(Functions.CMS.Configuration.Get("CaptchaValidationSite"), Functions.CMS.Configuration.Get("CaptchaSecretKey"), captcha, ip);
                    if (!captchaResult.Succeeded)
                        throw new Exception(Resources.Strings.Authentication_CaptchaRequired);
                }

                // Handle null parameters passed
                if (string.IsNullOrWhiteSpace(username))
                    throw new Exception(Resources.Strings.Membership_ArgumentNullException);

                // Get all available members
                using (var db = new DatabaseConnection())
                {
                    var member = await db.Users.Include(m => m.Role).Include(m => m.UserFields)
                                         .FirstOrDefaultAsync(m => !m.AdminAccess && !m.Disabled && m.UserName.Equals(username, StringComparison.CurrentCultureIgnoreCase));

                    // Check if the member exists with the UserName
                    if (member == null)
                    {
                        Thread.Sleep(2000);
                        throw new HttpRequestValidationException(Resources.Strings.Membership_InvalidLoginUser);
                    }

                    // Check if the password match
                    if (member.Password != null || !string.IsNullOrWhiteSpace(password))
                    {
                        if (!Security.Cryptography.Check(password, member.Password))
                            throw new UnauthorizedAccessException(Resources.Strings.Membership_InvalidCredentials);
                    }

                    // Check if the e-mail is valid
                    if (!string.IsNullOrWhiteSpace(member.Email) && Functions.CMS.Configuration.Get("RequiredEmailToBeValidated").Contains("true") && !member.Validated.HasValue)
                        throw new InvalidOperationException(Resources.Strings.Membership_EmailNotValidated);

                    // Login member in the App
                    Functions.CMS.Membership.SetAuthenticationMember(member);

                    // Return response to the user
                    return Json(new JsonResponse() { Result = new { Status = "OK", member.ChangePassword } }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (HttpRequestValidationException ex)
            {
                return Json(new JsonResponse() { Code = System.Net.HttpStatusCode.NonAuthoritativeInformation, Error = ex.AllMessages() }, JsonRequestBehavior.AllowGet);
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
                return Json(new JsonResponse() { Code = System.Net.HttpStatusCode.BadRequest, Error = ex.AllMessages() }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Method to change users password
        /// </summary>
        /// <param name="password">New password provided</param>
        /// <param name="confirmation">Confirmation of the password</param>
        /// <returns></returns>
        [HttpPost, Route("Autenticacao/Alterar-Senha")]
        public async Task<JsonResult> AlterPassword(string password, string confirmation)
        {
            try
            {
                // Validate if the user is not authenticated
                if (!Functions.CMS.Membership.IsAuthenticated)
                    throw new UnauthorizedAccessException(Resources.Strings.Membership_UserNotLogged);

                // Handle bypass parameter to fool hackers
                if (!string.IsNullOrWhiteSpace(Request.Form["bypass"]))
                    return Json(new JsonResponse() { Code = System.Net.HttpStatusCode.NoContent, Result = "bypass" }, JsonRequestBehavior.AllowGet);

                // Handle null parameters passed
                if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirmation))
                    throw new Exception(Resources.Strings.Membership_ArgumentNullException);

                // Handle null parameters passed
                if (password != confirmation)
                    throw new Exception(Resources.Strings.Membership_PasswordNotMatch);

                // Update available member
                using (var db = new DatabaseConnection())
                {
                    var user = Functions.CMS.Membership.User;
                    var member = await db.Users.Include(u => u.Role).Include(u => u.UserFields).FirstOrDefaultAsync(u => u.Id == user.Id);

                    // Update password and save changes
                    member.Password = Security.Cryptography.Encrypt(password);
                    member.ChangePassword = false;

                    await db.SaveChangesAsync();

                    // Update member var in session
                    user = member;

                    // Return response to the user
                    return Json(new JsonResponse() { Result = "OK" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                return Json(new JsonResponse() { Code = System.Net.HttpStatusCode.Unauthorized, Error = ex.AllMessages() }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new JsonResponse() { Code = System.Net.HttpStatusCode.BadRequest, Error = ex.AllMessages() }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Method to change users password
        /// </summary>
        /// <param name="password">New password provided</param>
        /// <param name="confirmation">Confirmation of the password</param>
        /// <returns></returns>
        [HttpPost, Route("Autenticacao/Dados-Perfil")]
        public async Task<JsonResult> UpdateProfile(Data.Model.User entity)
        {
            var sendToken = false;

            try
            {
                // Handle bypass parameter to fool hackers
                if (!string.IsNullOrWhiteSpace(Request.Form["bypass"]))
                    return Json(new JsonResponse() { Code = System.Net.HttpStatusCode.NoContent, Result = "bypass" }, JsonRequestBehavior.AllowGet);

                // Validate if the user is not authenticated
                var user = Functions.CMS.Membership.User;
                if (!Functions.CMS.Membership.IsAuthenticated)
                    throw new UnauthorizedAccessException(Resources.Strings.Membership_UserNotLogged);

                // Trigger event before execute the routine
                Functions.CMS.Events.Trigger("OnMemberUserUpdating", entity);

                // Update available member
                using (var db = new DatabaseConnection())
                {
                    var member = await db.Users.Include(u => u.Role).Include(u => u.UserFields).FirstOrDefaultAsync(u => u.Id == user.Id);

                    // Update allowed member fields
                    member.FirstName = entity.FirstName;
                    member.LastName = entity.LastName;
                    // Set e-mail validation
                    if (!string.Equals(member.Email, entity.Email, StringComparison.CurrentCultureIgnoreCase))
                    {
                        member.Email = entity.Email;
                        member.Validated = null;

                        // Trigger send token Notification to validate user
                        member.Token = Guid.NewGuid().ToString();
                        sendToken = true;
                    }

                    // Reset Password if is was provided
                    if (!string.IsNullOrWhiteSpace(entity.Password))
                    {
                        if (entity.Password != Request.Form["ConfirmPassword"])
                            throw new Exception(Resources.Strings.Membership_PasswordNotMatch);

                        member.Password = Security.Cryptography.Encrypt(entity.Password);
                        member.ChangePassword = false;
                    }

                    // Save custom fields
                    foreach (var entityField in entity.UserFields)
                    {
                        var memberField = member.UserFields.FirstOrDefault(f => f.Name == entityField.Name);
                        if (memberField != null)
                            memberField.Value = entityField.Value;
                        else
                            member.UserFields.Add(new UserField() { Name = entityField.Name, Value = entityField.Value });
                    }

                    // Data completed so, keep going
                    member.Completed = true;
                    if (member.CompletedAt == null)
                        member.CompletedAt = DateTime.Now;

                    // Update profile picture
                    var file = Request.Files.Get(nameof(member.ProfilePicture));
                    if (file == null)
                        member.ProfilePicture = entity.ProfilePicture;
                    else
                    {
                        try
                        {
                            // Store profile picture
                            var image = Image.FromStream(file.InputStream);

                            // Check if it should be optimized
                            var optimize = Functions.CMS.Configuration.Get("AutoScaleProfilePicture");
                            if (!string.IsNullOrWhiteSpace(optimize) && optimize != "0")
                                image = image.ScaleImage(200, 200);

                            // Locate and create the folder image path
                            var imagePath = Functions.CMS.Configuration.Get("ProfileImagePath");
                            var path = HostingEnvironment.MapPath(imagePath);
                            Directory.CreateDirectory(path);

                            // Create filename and save data to disk
                            var name = $"{Guid.NewGuid()}{System.IO.Path.GetExtension(file.FileName)}";
                            var fileName = System.IO.Path.Combine(path, name);
                            image.Save(fileName);

                            // Remove old member profile
                            if (!string.IsNullOrWhiteSpace(member.ProfilePicture))
                                System.IO.File.Delete(HostingEnvironment.MapPath(member.ProfilePicture));

                            // Store in member profile picture information
                            member.ProfilePicture = $"{imagePath.Replace("~", "")}/{name}";
                        }
                        catch (Exception imgEx)
                        {
                            var parameters = new
                            {
                                Exception = imgEx,
                                Controller = this.Request.RequestContext.RouteData.DataTokens["controller"]?.ToString(),
                                Action = this.Request.RequestContext.RouteData.DataTokens["action"]?.ToString(),
                                Url = this.Request.Url.ToString(),
                                Entity = entity
                            };
                            log.LogRequest(parameters);
                        }
                    }

                    // Update member var in session
                    Functions.CMS.Membership.Authorization.User = member;

                    // Send Token if has to send
                    if (sendToken)
                    {
                        await this.TriggerNotification("ValidarEmail", $"{Functions.CMS.Configuration.SiteName} / {Resources.Strings.MailService_ActivateEmailSubject}", new[] { member.Email });
                        // Clear authentication
                        Session.Set<MembershipAuthentication>(null);
                    }

                    // Apply Changes to the database
                    await db.SaveChangesAsync();

                    // Trigger event before execute the routine
                    Functions.CMS.Events.Trigger("OnMemberUserUpdated", member);

                    // Clear Cache information
                    Functions.CMS.ClearCache(typeof(Functions.Internal.User).FullName);

                    // Return response to the user
                    return Json(new JsonResponse() { Result = "OK" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                return Json(new JsonResponse() { Code = System.Net.HttpStatusCode.Unauthorized, Error = ex.AllMessages() }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var parameters = new
                {
                    Exception = ex,
                    Controller = this.Request.RequestContext.RouteData.DataTokens["controller"]?.ToString(),
                    Action = this.Request.RequestContext.RouteData.DataTokens["action"]?.ToString(),
                    Url = this.Request.Url.ToString(),
                    Entity = entity
                };
                log.LogRequest(parameters);
                return Json(new JsonResponse() { Code = System.Net.HttpStatusCode.BadRequest, Error = ex.AllMessages() }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Method to change users password
        /// </summary>
        /// <returns></returns>
        [HttpPost, Route("Autenticacao/Cadastrar")]
        public async Task<JsonResult> CreateProfile(Data.Model.User entity, UserSocial social = null)
        {
            try
            {
                // Handle bypass parameter to fool hackers
                if (!string.IsNullOrWhiteSpace(Request.Form["bypass"]))
                    return Json(new JsonResponse() { Code = System.Net.HttpStatusCode.NoContent, Result = "bypass" }, JsonRequestBehavior.AllowGet);

                if (entity.Password != Request.Form["ConfirmPassword"])
                    throw new Exception(Resources.Strings.Membership_PasswordNotMatch);

                // Trigger event before execute the routine
                Functions.CMS.Events.Trigger("OnMemberUserCreating", entity);

                // Check if access token is provided. Otherwise, clean social
                if (social != null && string.IsNullOrWhiteSpace(social.AccessToken))
                    social = null;

                // Validate social login information
                if (social != null)
                {
                    await SocialLoginHelper.EnsureAccessTokenBelongsUser(social.Type, social.SourceId, entity.UserName, social.AccessToken);

                    // When social login is requested, generate an random password
                    entity.Password = Guid.NewGuid().ToString().Substring(12);
                }

                // Validate captcha
                if (Functions.CMS.Configuration.EnforceCaptcha && social == null)
                {
                    var ip = MvcApplication.GetClientIp();
                    var captcha = Request.Form["g-recaptcha-response"];
                    if (string.IsNullOrWhiteSpace(captcha))
                        throw new Exception(Resources.Strings.Authentication_CaptchaRequired);

                    var captchaResult = Recaptcha.Validate(Functions.CMS.Configuration.Get("CaptchaValidationSite"), Functions.CMS.Configuration.Get("CaptchaSecretKey"), captcha, ip);
                    if (!captchaResult.Succeeded)
                        throw new Exception(Resources.Strings.Authentication_CaptchaRequired);
                }

                // Update available member
                using (var db = new DatabaseConnection())
                {
                    var member = await db.Users.Include(u => u.Role).Include(u => u.UserFields).FirstOrDefaultAsync(u => u.Email == entity.Email);
                    if (member != null)
                        throw new UnauthorizedAccessException(Resources.Strings.Membership_UserAlreadyExists);

                    // Create member reference
                    member = new Data.Model.User
                    {
                        // Update allowed member fields
                        FirstName = entity.FirstName,
                        LastName = entity.LastName,
                        UserName = entity.Email,
                        Email = entity.Email,
                        Token = Guid.NewGuid().ToString(),
                        Password = Security.Cryptography.Encrypt(entity.Password),
                        AdminAccess = false,
                        IdRole = Functions.CMS.Configuration.Get("DefaultMemberRole")?.AsInt() ?? throw new Exception("Role padrão não cadastrada.")
                    };

                    // Save custom fields
                    foreach (var entityField in entity.UserFields)
                    {
                        var memberField = member.UserFields.FirstOrDefault(f => f.Name == entityField.Name);
                        if (memberField != null)
                            memberField.Value = entityField.Value;
                        else
                            member.UserFields.Add(new UserField() { Name = entityField.Name, Value = entityField.Value });
                    }

                    // Apply Changes to the database
                    db.Users.Add(member);
                    await db.SaveChangesAsync();

                    // Clear authentication
                    Session.Set<MembershipAuthentication>(null);

                    // If is not login social, send validation mail
                    if (social == null)
                    {
                        TempData["User"] = member;
                        Functions.CMS.Membership.SelectedUser = member;
                        await this.TriggerNotification("ValidarEmail", $"{Functions.CMS.Configuration.SiteName} / {Resources.Strings.MailService_ActivateEmailSubject}", new[] { member.Email });
                    }
                    else
                    {
                        // Clear validation data
                        member.Validated = DateTime.Now;
                        member.Token = null;

                        // Social login save in the database
                        social.IdUser = member.Id;
                        db.UserSocial.Add(social);

                        // Save data
                        await db.SaveChangesAsync();

                        // Set autologin
                        member = await db.Users.Include(u => u.Role).Include(u => u.UserFields).FirstOrDefaultAsync(u => u.Id == member.Id);
                        Functions.CMS.Membership.SetAuthenticationMember(member);
                    }

                    // Trigger event after execute the routine
                    Functions.CMS.Events.Trigger("OnMemberUserCreated", member);

                    // Clear cache to allow the field be retrieved
                    Functions.CMS.ClearCache(typeof(Functions.Internal.User).FullName);
                    Functions.CMS.ClearCache(typeof(Functions.Internal.Membership).FullName);

                    // Return response to the user
                    return Json(new JsonResponse() { Result = "OK" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                return Json(new JsonResponse() { Code = System.Net.HttpStatusCode.Unauthorized, Error = ex.AllMessages() }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var parameters = new
                {
                    Exception = ex,
                    Controller = this.Request.RequestContext.RouteData.DataTokens["controller"]?.ToString(),
                    Action = this.Request.RequestContext.RouteData.DataTokens["action"]?.ToString(),
                    Url = this.Request.Url.ToString(),
                    Entity = entity,
                    Social = social
                };
                log.LogRequest(parameters);
                return Json(new JsonResponse() { Code = System.Net.HttpStatusCode.BadRequest, Error = ex.AllMessages() }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Method to Validate the current token
        /// </summary>
        /// <param name="token">Token Value to be validated</param>
        /// <returns></returns>
        [HttpGet, Route("Autenticacao/Validar-Usuario/{token}")]
        public async Task<JsonResult> ValidateToken(string token)
        {
            try
            {
                // Handle null parameters passed
                if (string.IsNullOrWhiteSpace(token))
                    throw new Exception(Resources.Strings.Membership_ArgumentNullException);

                // Update available member
                using (var db = new DatabaseConnection())
                {
                    var member = await db.Users.Include(m => m.Role).Include(m => m.UserFields).FirstOrDefaultAsync(u => u.Token == token);
                    if (member == null)
                        throw new UnauthorizedAccessException(Resources.Strings.Membership_InvalidToken);

                    // Update password and save changes
                    member.Token = null;
                    member.Validated = DateTime.Now;

                    await db.SaveChangesAsync();

                    // Validate if should Auto-Login
                    if (Functions.CMS.Configuration.Get("AutoLoginOnMemberValidate").Contains("true"))
                        Functions.CMS.Membership.SetAuthenticationMember(member);

                    // Clear Cache information
                    Functions.CMS.ClearCache(typeof(Functions.Internal.User).FullName);
                    Functions.CMS.ClearCache(typeof(Functions.Internal.Membership).FullName);

                    // Return response to the user
                    return Json(new JsonResponse() { Result = "OK" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                return Json(new JsonResponse() { Code = System.Net.HttpStatusCode.Unauthorized, Error = ex.AllMessages() }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new JsonResponse() { Code = System.Net.HttpStatusCode.BadRequest, Error = ex.AllMessages() }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Method to Validate the current login social in the service
        /// </summary>
        /// <param name="type">Indicates the login type source to be validated</param>
        /// <param name="user">User Id on provider</param>
        /// <param name="user">User name to validate in the service</param>
        /// <param name="token">Token to autenticate and validate the user</param>
        /// <returns></returns>
        [HttpPost, Route("Autenticacao/Validar-Login-Social")]
        public async Task<JsonResult> ValidateSocialLogin(string type, string userId, string user, string token)
        {
            try
            {
                // Handle null parameters passed
                if (string.IsNullOrWhiteSpace(token))
                    throw new Exception(Resources.Strings.Membership_ArgumentNullException);
                if (string.IsNullOrWhiteSpace(type))
                    throw new Exception(Resources.Strings.Membership_ArgumentNullException);
                if (string.IsNullOrWhiteSpace(user))
                    throw new Exception(Resources.Strings.Membership_ArgumentNullException);

                // Update available member
                using (var db = new DatabaseConnection())
                {
                    // Check if the user is valid
                    var member = await db.Users.Include(m => m.Role)
                                               .Include(m => m.UserFields)
                                               .Include(m => m.UserSocial)
                                               .FirstOrDefaultAsync(u => u.UserName.Equals(user));
                    if (member == null)
                        throw new InvalidDataException(Resources.Strings.Membership_InvalidLoginUser);

                    // Ensure that access token belongs to the user
                    await SocialLoginHelper.EnsureAccessTokenBelongsUser(type, userId, user, token);

                    // Validate if the token exists in the database
                    if (!member.UserSocial.Any(m => m.AccessToken == token))
                    {
                        var socialLogin = new UserSocial()
                        {
                            AccessToken = token,
                            IdUser = member.Id,
                            SourceId = userId,
                            Type = type,
                        };

                        // Add new social login in the database
                        db.UserSocial.Add(socialLogin);
                        await db.SaveChangesAsync();

                        // Set member social Login information
                        member.UserSocial.Add(socialLogin);
                    }

                    // Set user as authenticated
                    Functions.CMS.Membership.SetAuthenticationMember(member);

                    // Return response to the user
                    return Json(new JsonResponse() { Result = "OK" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (InvalidDataException ex)
            {
                return Json(new JsonResponse() { Code = System.Net.HttpStatusCode.NoContent, Error = ex.AllMessages() }, JsonRequestBehavior.AllowGet);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Json(new JsonResponse() { Code = System.Net.HttpStatusCode.Unauthorized, Error = ex.AllMessages() }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new JsonResponse() { Code = System.Net.HttpStatusCode.BadRequest, Error = ex.AllMessages() }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Method to Send Email Password Reset to the User
        /// </summary>
        /// <param name="user">Username to identify the user in the database</param>
        /// <returns></returns>
        [HttpGet, Route("Autenticacao/Redefinir-Senha")]
        public async Task<JsonResult> ResetPassword(string user)
        {
            try
            {
                // Handle null parameters passed
                if (string.IsNullOrWhiteSpace(user))
                    throw new Exception(Resources.Strings.Membership_ArgumentNullException);

                // Update available member
                using (var db = new DatabaseConnection())
                {
                    var member = await db.Users.FirstOrDefaultAsync(u => u.UserName == user && !u.AdminAccess && !u.Disabled);
                    if (member == null || string.IsNullOrWhiteSpace(member.Email))
                        throw new UnauthorizedAccessException(Resources.Strings.Membership_InvalidUsername);

                    // Reset user password to be remembered
                    var password = Convert.ToBase64String(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()), Base64FormattingOptions.None).Substring(0, 8);
                    member.Password = Security.Cryptography.Encrypt(password);
                    member.ChangePassword = true;

                    await db.SaveChangesAsync();

                    // Store new Password and user member in temp data
                    TempData["ResetPassword"] = password;
                    TempData["MemberRequest"] = member;

                    await this.TriggerNotification("RedefinirSenha", $"{Functions.CMS.Configuration.SiteName} / {Resources.Strings.MailService_ResetPasswordSubject}", new[] { member.Email });
                    RefreshUserCache();

                    // Return response to the user
                    return Json(new JsonResponse() { Result = "OK" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new JsonResponse() { Code = System.Net.HttpStatusCode.BadRequest, Error = ex.AllMessages() }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Method to Logoff the current user
        /// </summary>
        /// <param name="redirectUrl">Redirect Url Parameter</param>
        /// <returns></returns>
        [HttpGet, Route("Autenticacao/Sair")]
        public ActionResult Logoff(string redirectUrl)
        {
            // Clear session
            if (Functions.CMS.Membership.IsAuthenticated)
                Session.Set<MembershipAuthentication>(null);

            // Redirect to home if redirect url was not provided
            if (string.IsNullOrWhiteSpace(redirectUrl))
                return Redirect("/");

            return Redirect(redirectUrl);
        }

        /// <summary>
        /// Method to request the user to validate the e-mail
        /// </summary>
        /// <returns></returns>
        [Route("Autenticacao/Requisitar-Validacao")]
        public async Task<JsonResult> RequestMailValidationUser(int id)
        {
            try
            {
                var user = Functions.CMS.User.Users().FirstOrDefault(u => u.Id == id);
                if ((user.Validated.HasValue))
                    return Json(new { status = "ok" }, JsonRequestBehavior.AllowGet);

                // Set token to the user
                if (string.IsNullOrWhiteSpace(user.Token))
                    using (var db = new DatabaseConnection())
                    {
                        var token = Guid.NewGuid().ToString();
                        user.Token = token;
                        db.Database.ExecuteSqlCommand("UPDATE btz_user SET Token = @p1 WHERE Id = @p0", user.Id, token);
                    }

                // Set selected user
                Functions.CMS.Membership.SelectedUser = user;
                TempData["User"] = user;

                // Trigger e-mail to validate user
                await this.TriggerNotification("ValidarEmail", $"{Functions.CMS.Configuration.SiteName} / {Resources.Strings.MailService_ActivateEmailSubject}", new[] { user.Email });
                return Json(new { status = "ok" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { error = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Function extracted from the request mail validation object to be used in another place
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task TriggerMailValidation(User user)
        {
            // Set selected user
            Functions.CMS.Membership.SelectedUser = user;
            TempData["User"] = user;

            // Trigger e-mail to validate user
            await this.TriggerNotification("ValidarEmail", $"{Functions.CMS.Configuration.SiteName} / {Resources.Strings.MailService_ActivateEmailSubject}", new[] { user.Email });
        }

        /// <summary>
        /// Method to set the email for the user with the new password provided
        /// </summary>
        /// <param name="member"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task TriggerResetPasswordMail(User member, string password)
        {
            // Store new Password and user member in temp data
            TempData["ResetPassword"] = password;
            TempData["MemberRequest"] = member;

            await this.TriggerNotification("RedefinirSenha", $"{Functions.CMS.Configuration.SiteName} / {Resources.Strings.MailService_ResetPasswordSubject}", new[] { member.Email });
        }

        /// <summary>
        /// Support method to clear user cache
        /// </summary>
        internal static void RefreshUserCache()
        {
            Functions.CMS.ClearCache(typeof(Functions.Internal.User).FullName);
            Functions.CMS.ClearCache(typeof(Functions.Internal.Membership).FullName);
        }
    }
}