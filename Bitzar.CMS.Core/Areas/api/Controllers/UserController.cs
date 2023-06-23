using Bitzar.CMS.Core.Areas.api.Helpers;
using Bitzar.CMS.Core.Functions;
using Bitzar.CMS.Core.Helper;
using Bitzar.CMS.Core.Resources;
using Bitzar.CMS.Data.Model;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;

namespace Bitzar.CMS.Core.Areas.api.Controllers
{
    [ApiAdminAuthorization]
    [RoutePrefix("api/v1")]
    public class UserController : BaseController
    {



        /// <summary>
        /// Endpoint to list users available
        /// </summary>
        /// <param name="idRole"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("users")]
        public async Task<HttpResponseMessage> Users(int? idRole = null)
        {
            try
            {
                var response = Functions.CMS.User.Users();

                if(idRole != null)
                    response = response.FindAll(x => x.IdRole == idRole);

                return await CreateResponse(response);
            }
            catch (Exception ex)
            {
                return await this.HandleException(ex);
            }
        }

        /// <summary>
        /// Endpoint to get user
        /// </summary>
        /// <param name="idUser"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("users/{idUser}")]
        public async Task<HttpResponseMessage> GetProfile(int idUser)
        {
            try
            {
                using (var db = new DatabaseConnection())
                {
                    var user = Functions.CMS.User.Users().FirstOrDefault(f => f.Id == idUser)
                        ?? throw new ValidationException(Strings.Authentication_UserNotFound);

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
            }
            catch (Exception ex)
            {
                return await this.HandleException(ex);
            }
        }

        /// <summary>
        /// Endpoint to uptdate user
        /// </summary>
        /// <param name="idUser"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("users/update-profile/{idUser}")]
        public async Task<HttpResponseMessage> UpdateProfile(dynamic body, int? idUser)
        {
            try
            {
                using (var db = new DatabaseConnection())
                {
                    var user = Functions.CMS.User.Users().FirstOrDefault(f => f.Id == idUser);

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
            }
            catch (Exception ex)
            {
                return await this.HandleException(ex);
            }
        }
    }
}