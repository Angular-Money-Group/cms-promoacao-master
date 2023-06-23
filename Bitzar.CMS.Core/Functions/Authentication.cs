using Bitzar.CMS.Core.Areas.api.Helpers;
using Bitzar.CMS.Core.Areas.api.Models;
using Bitzar.CMS.Core.Helper;
using Bitzar.CMS.Core.Resources;
using Bitzar.CMS.Data;
using Bitzar.CMS.Data.Model;
using System;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.WebPages;

namespace Bitzar.CMS.Core.Functions
{
    /// <summary>
    /// Function responsible for the web api authentication rule
    /// </summary>
    public static class Authentication
    {
        /// <summary>
        /// Method to process the Login user.
        /// </summary>
        /// <param name="username">Username to be validated</param>
        /// <param name="password">Password to grant access to the user</param>
        /// <returns></returns>
        internal static User Login(string username, string password)
        {
            // Get all available members
            using (var db = new DatabaseConnection())
            {
                var member = db.Users
                               .AsNoTracking()
                               .Include(u => u.Role)
                               .Include(u => u.UserFields)
                               .FirstOrDefault(u => !u.Disabled && !u.Deleted && u.UserName == username);

                // Check if the member exists with the UserName
                if (member == null)
                    throw new UnauthorizedAccessException(Strings.Membership_InvalidLoginUser);

                // Check if the password match
                if (member.Password != null || !string.IsNullOrWhiteSpace(password))
                    if (!Security.Cryptography.Check(password, member.Password))
                        throw new UnauthorizedAccessException(Strings.Membership_InvalidCredentials);

                // Check if the e-mail is valid
                if (!string.IsNullOrWhiteSpace(member.Email) && CMS.Configuration.Get("RequiredEmailToBeValidated").Contains("true") && !member.Validated.HasValue)
                    throw new ValidationException(Strings.Membership_EmailNotValidated);

                db.Users.Attach(member);

                // Set the last login info
                member.LastLogin = DateTime.Now;
                db.SaveChanges();

                return member;
            }
        }

        /// <summary>
        /// Method responsible for changing the user's password
        /// </summary>
        /// <param name="member"></param>
        /// <param name="password"></param>
        /// <param name="confirmation"></param>
        /// <returns></returns>
        internal static async Task<User> AlterPassword(User member, string password, string confirmation)
        {
            // Handle null parameters passed
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirmation))
                throw new ArgumentException(Strings.Membership_ArgumentNullException);

            // Handle null parameters passed
            if (password != confirmation)
                throw new InvalidOperationException(Strings.Membership_PasswordNotMatch);

            // Handle password equal to persisted in the database
            if (Security.Cryptography.Check(password, member.Password))
                throw new InvalidOperationException(Strings.Membership_SamePasswordCannotBeUpdated);

            using (var db = new DatabaseConnection())
            {
                var user = await db.Users.FindAsync(member.Id);

                user.Password = Security.Cryptography.Encrypt(password);
                user.ChangePassword = false;

                await db.SaveChangesAsync();
            }

            // Clear user cache if need
            RefreshUserCache();

            return member;
        }

        /// <summary>
        /// Method responsible for reset the user's password
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        internal static async Task<User> ResetPassword(User member)
        {
            if (member == null || string.IsNullOrWhiteSpace(member.Email))
                throw new ArgumentException(Strings.Membership_InvalidUsername);

            using (var db = new DatabaseConnection())
            {
                var password = Convert.ToBase64String(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()), Base64FormattingOptions.None).Substring(0, 8);
                var user = await db.Users.FirstOrDefaultAsync(u => u.Id == member.Id);

                // Reset user password to be remembered
                user.Password = Security.Cryptography.Encrypt(password);
                user.ChangePassword = true;

                await db.SaveChangesAsync();

                // Trigger email
                await MailContext().TriggerResetPasswordMail(user, password);
                RefreshUserCache();

                return member;
            }
        }

        /// <summary>
        /// Method responsible for the user's email validation request
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        internal static async Task RequestMailValidationUser(User member)
        {
            if (member == null)
                throw new ArgumentNullException(nameof(member));

            if ((member.Validated.HasValue))
                return;

            // Set token to the user
            using (var db = new DatabaseConnection())
            {
                var user = await db.Users.FindAsync(member.Id);
                user.Token = Guid.NewGuid().ToString();

                await db.SaveChangesAsync();

                // Trigger email
                await MailContext().TriggerMailValidation(user);
            }

            // Clear cache for the users data
            RefreshUserCache();
        }

        /// <summary>
        /// Method responsible for creating the user profile
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        internal static async Task<User> CreateProfile(UserModel model, bool adminAccess = false, int? defaultRole = null)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            // Check if access token is provided. Otherwise, clean social
            if (model.Social != null && string.IsNullOrWhiteSpace(model.Social.AccessToken))
                model.Social = null;

            // Validate social login information
            if (model.Social != null)
            {
                await SocialLoginHelper.EnsureAccessTokenBelongsUser(model.Social.Type, model.Social.SourceId, model.UserName, model.Social.AccessToken);
                // When social login is requested, generate an random password
                model.Password = Guid.NewGuid().ToString().Substring(16);
            }

            using (var db = new DatabaseConnection())
            {
                // Check if the member email is altery an the database
                if (db.Users.Any(u => u.Email == model.Email) || db.Users.Any(u => u.UserName == model.UserName))
                    throw new InvalidDataException(Strings.Membership_UserAlreadyExists);

                // Create member reference
                if (defaultRole == null && CMS.Configuration.Get("DefaultMemberRole").AsInt() != 0)
                    defaultRole = CMS.Configuration.Get("DefaultMemberRole").AsInt();

                var member = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    UserName = model.UserName,
                    Email = model.Email,
                    Token = Guid.NewGuid().ToString(),
                    Password = Security.Cryptography.Encrypt(model.Password),
                    AdminAccess = adminAccess,
                    IdRole = defaultRole ?? throw new ConfigurationErrorsException(Strings.Authentication_DefaultRoleNotProvided)
                };

                // Save custom fields
                if (model.UserFields != null && model.UserFields.Any())
                    foreach (var entityField in model.UserFields)
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

                // If is not login social, send validation mail
                if (model.Social == null)
                    model.SendMail = CMS.Configuration.Get("RequiredEmailToBeValidated").Contains("true");
                else
                {
                    // Clear validation data
                    member.Validated = DateTime.Now;
                    member.Token = null;

                    // Social login save in the database
                    model.Social.UserId = member.Id;
                    db.UserSocial.Add
                    (
                        new UserSocial
                        {
                            IdUser = member.Id,
                            Type = model.Social.Type,
                            SourceId = model.Social.SourceId,
                            AccessToken = model.Social.AccessToken
                        }
                    );

                    // Save data
                    await db.SaveChangesAsync();
                }

                // Clear cache to reload user data
                RefreshUserCache();

                // Method to trigger mail validation
                if (model.SendMail)
                    await RequestMailValidationUser(member);

                // Return response to the user
                return member;
            }
        }

        /// <summary>
        /// Method responsible for updating the user profile
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        internal static async Task<User> UpdateProfile(UserModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            // Lookup the user and set the default values
            var sendValidationMail = false;
            var user = CMS.User.Users().FirstOrDefault(u => u.Id == model.Id);

            // Apply data in the database
            using (var db = new DatabaseConnection())
            {
                db.Users.Attach(user);

                // Start update the values
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;

                if (!string.IsNullOrWhiteSpace(model.Email) && !string.Equals(user.Email, model.Email, StringComparison.CurrentCultureIgnoreCase))
                {
                    // Check if the e-mail aready exists for another user
                    if (AuthenticationHelper.CheckIfEmailExist(model.Email))
                        throw new InvalidDataException(Strings.Membership_EmailUpdateNotAllowed);

                    // Update the mail data
                    user.Email = model.Email;
                    user.Validated = null;

                    // Trigger send token Notification to validate user
                    sendValidationMail = true;
                }

                // Reset Password if is was provided
                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    // Validate if the password and confirmation match
                    if (model.Password != model.ConfirmPassword)
                        throw new DataMisalignedException(Strings.Membership_PasswordNotMatch);

                    // Validate if is the same password provided
                    if (Security.Cryptography.Check(model.Password, user.Password))
                        throw new InvalidOperationException(Strings.Membership_SamePasswordCannotBeUpdated);

                    user.Password = Security.Cryptography.Encrypt(model.Password);
                    user.ChangePassword = false;
                }

                // Set user fields
                if (model.UserFields != null && model.UserFields.Any())
                    foreach (var field in model.UserFields)
                    {
                        var memberField = user.UserFields.FirstOrDefault(f => f.Name == field.Name);
                        if (memberField != null)
                            memberField.Value = field.Value;
                        else
                            user.UserFields.Add(new UserField { Name = field.Name, Value = field.Value });
                    }

                // Data completed so, keep going
                user.Completed = true;
                if (user.CompletedAt == null)
                    user.CompletedAt = DateTime.Now;

                await db.SaveChangesAsync();
            }

            // Send validation mail
            if (sendValidationMail)
                await RequestMailValidationUser(user);

            RefreshUserCache();
            return user;
        }

        /// <summary>
        /// Endpoint responsible for updating the user profile image
        /// </summary>
        /// <param name="file"></param>
        /// <param name="member"></param>
        /// <returns></returns>
        internal static async Task<User> UpdateProfilePicture(HttpPostedFile file, User member)
        {
            if (member == null)
                throw new ArgumentNullException(nameof(member));

            // Save the image in the disk
            var image = ProfilePictureAction(file, member);
            using (var db = new DatabaseConnection())
            {
                var user = await db.Users.FindAsync(member.Id);
                user.ProfilePicture = image;

                await db.SaveChangesAsync();

                // Invalidate cache service to be returned.
                RefreshUserCache();
                return member;
            }
        }

        /// <summary>
        /// Support method to update user image
        /// </summary>
        /// <param name="file"></param>
        /// <param name="member"></param>
        /// <returns></returns>
        internal static string ProfilePictureAction(HttpPostedFile file, User member)
        {
            try
            {
                string name = string.Empty;
                string fileName = string.Empty;

                // Locate and create the folder image path
                var imagePath = CMS.Configuration.Get("ProfileImagePath");
                var path = HostingEnvironment.MapPath(imagePath);
                Directory.CreateDirectory(path);

                if (!string.IsNullOrEmpty(member.ProfilePicture))
                {
                    name = member.ProfilePicture;
                    fileName = System.IO.Path.Combine(path, name);
                    File.Delete(fileName);
                }

                var image = Image.FromStream(file.InputStream);

                // Check if it should be optimized
                var optimize = CMS.Configuration.Get("AutoScaleProfilePicture");
                if (!string.IsNullOrWhiteSpace(optimize) && optimize != "0")
                    image = image.ScaleImage(200, 200);

                // Create filename and save data to disk
                name = $"{Guid.NewGuid()}{System.IO.Path.GetExtension(file.FileName)}";
                fileName = System.IO.Path.Combine(path, name);
                image.Save(fileName);

                return name;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.AllMessages());
            }
        }

        /// <summary>
        /// Internal method to create the mail context (Controller) 
        /// responsible to trigger mails
        /// </summary>
        /// <returns></returns>
        internal static Controllers.AuthenticationController MailContext()
        {
            /*
             * Trigger mail notification calling the controller from the main service.
             * It's demanding to have the service controller instance due the fact the mail template should be rendered
             * and it's not possible to send directly without the preprocessing handling
             */
            var controller = new Controllers.AuthenticationController();
            controller.ControllerContext = new System.Web.Mvc.ControllerContext(HttpContext.Current.Request.RequestContext, controller);

            return controller;
        }


        /// <summary>
        /// Support method to clear user cache
        /// </summary>
        internal static void RefreshUserCache(){
            CMS.ClearCache(typeof(Functions.Internal.User).FullName);
            CMS.ClearCache(typeof(Functions.Internal.Membership).FullName);
        }
    }
}
