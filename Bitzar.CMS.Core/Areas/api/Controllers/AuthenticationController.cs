using Bitzar.CMS.Core.Areas.api.Helpers;
using Bitzar.CMS.Core.Areas.api.Models;
using Bitzar.CMS.Core.Functions;
using Bitzar.CMS.Core.Resources;
using Bitzar.CMS.Data.Model;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Data.Entity.Core;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;

namespace Bitzar.CMS.Core.Areas.api.Controllers
{
    [RoutePrefix("api/v1")]
    public class AuthenticationController : BaseController
    {
        /// <summary>
        /// Endpoint responsible for changing the user's password
        /// </summary>
        /// <param name="body.password"></param>
        /// <param name="body.confirmation"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("authentication/alter-password")]
        public async Task<HttpResponseMessage> AlterPassword([FromBody] dynamic body)
        {
            try
            {
                // Lookup for the authenticated Used
                var id = AuthenticationHelper.GetCurrentUserId();

                //Lookup for the user in the service
                var user = Functions.CMS.User.Users().FirstOrDefault(u => u.Id == id)
                    ?? throw new UnauthorizedAccessException(Strings.Membership_UserNotLogged); ;

                // Validate if the user is blocked
                if (user.Disabled || user.Deleted)
                    throw new UnauthorizedAccessException(Strings.Membership_UserBlocked);

                var member = await Authentication.AlterPassword(user, (string)body.password, (string)body.confirmation);

                // Create return response to the caller
                return await CreateResponse(Strings.Membership_PasswordUpdated);
            }
            catch (Exception ex)
            {
                return await this.HandleException(ex);
            }
        }

        /// <summary>
        /// Endpoint responsible for reset the user's password
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("authentication/reset-password")]
        public async Task<HttpResponseMessage> ResetPassword(string username)
        {
            try
            {
                // Get the user to be reseted
                var user = Functions.CMS.User.Users().FirstOrDefault(u => u.UserName.Equals(username, StringComparison.CurrentCultureIgnoreCase));
                var member = await Authentication.ResetPassword(user);

                return await CreateResponse(Strings.Membership_PasswordUpdated);
            }
            catch (Exception ex)
            {
                return await this.HandleException(ex);
            }
        }

        /// <summary>
        /// Endpoint responsible for the user's mail validation request
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("authentication/mail-validation-user")]
        public async Task<HttpResponseMessage> RequestMailValidationUser(string username)
        {
            try
            {
                // Get the user to be validated
                var user = Functions.CMS.User.Users().FirstOrDefault(u => u.UserName.Equals(username, StringComparison.CurrentCultureIgnoreCase));
                if ((user.Validated.HasValue))
                    return await CreateResponse(Strings.Membership_EmailAlreadyValidated);

                // Method to trigger mail validation
                await Authentication.RequestMailValidationUser(user);

                // Result data
                return await CreateResponse(Strings.Membership_ValidationEmailSent);
            }
            catch (Exception ex)
            {
                return await this.HandleException(ex);
            }
        }

        /// <summary>
        /// Endpoint responsible for creating the user profile
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("authentication/create-profile")]
        public async Task<HttpResponseMessage> CreateProfile(dynamic body)
        {
            try
            {
                var userModel = AuthenticationHelper.CreateUserModel(body);
                Functions.CMS.Events.Trigger("OnMemberUserCreating", userModel);

                var member = await Authentication.CreateProfile(userModel);

                return await CreateResponse(member);
            }
            catch (Exception ex)
            {
                return await this.HandleException(ex);
            }
        }

        /// <summary>
        /// Endpoint responsible for updating the user profile
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("authentication/update-profile")]
        public async Task<HttpResponseMessage> ProfileUpdate(dynamic body)
        {
            try
            {
                var user = AuthenticationHelper.GetUser();

                // Update profile
                var userModel = AuthenticationHelper.CreateUserModel(body, user);

                // Ensure the model updates the same user data.
                userModel.Id = user.Id;

                var member = await Authentication.UpdateProfile(userModel);

                // Return response to the user
                return await CreateResponse(new
                {
                    member.Id,
                    member.UserName,
                    member.FirstName,
                    member.LastName,
                    member.Email,
                    member.LastLogin,
                    member.Validated,
                    member.Token,
                    member.ProfilePicture,
                    member.ChangePassword,
                    member.Completed,
                    member.CompletedAt,
                    member.Disabled,
                    member.IdRole,
                    member.IdParent
                });
            }
            catch (Exception ex)
            {
                return await this.HandleException(ex);
            }
        }

        /// <summary>
        /// Endpoint responsible for getting user's profile information
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("authentication/profile")]
        public async Task<HttpResponseMessage> GetProfile()
        {
            try
            {
                var user = AuthenticationHelper.GetUser();

                // Return response to the user
                return await CreateResponse(new
                {
                    user.Id,
                    user.UserName,
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    user.LastLogin,
                    user.Validated,
                    user.Token,
                    user.ProfilePicture,
                    user.ChangePassword,
                    user.Completed,
                    user.CompletedAt,
                    user.Disabled,
                    user.IdRole,
                    user.IdParent,
                    user.AdminAccess,
                    Role = user.Role?.Description,
                    Fields = user.UserFields.Select(f => new { f.Name, f.Value }).ToArray(),
                    Social = user.UserSocial.Select(s => new { s.Id, s.SourceId, s.Type, s.AccessToken, s.Data }).ToArray()
                });
            }
            catch (Exception ex)
            {
                return await this.HandleException(ex);
            }
        }
    }
}