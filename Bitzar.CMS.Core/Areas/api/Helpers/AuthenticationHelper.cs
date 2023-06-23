using Bitzar.CMS.Core.Areas.api.Models;
using Bitzar.CMS.Core.Resources;
using Bitzar.CMS.Data;
using Bitzar.CMS.Data.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Claims;
using System.Web;

namespace Bitzar.CMS.Core.Areas.api.Helpers
{
    /// <summary>
    /// Support Helper: Authentication
    /// </summary>
    public static class AuthenticationHelper
    {
        /// <summary>
        /// Support helper for Web API
        /// </summary>
        /// <returns></returns>
        public static User GetUser()
        {
            var id = GetCurrentUserId();

            //Lookup for the user in the service
            var user = Functions.CMS.User.Users().FirstOrDefault(u => u.Id == id)
                ?? throw new UnauthorizedAccessException(Strings.Membership_UserNotLogged); ;

            // Validate if the user is blocked
            if (user.Disabled || user.Deleted)
                throw new UnauthorizedAccessException(Strings.Membership_UserBlocked);

            // Check if the user should be validated before proceed
            if (Functions.CMS.Configuration.Get("RequiredEmailToBeValidated").Contains("true") && !user.AdminAccess && !user.Validated.HasValue)
                throw new ValidationException(Strings.Membership_EmailNotValidated);

            // Validate if the user has pending password change
            if (user.ChangePassword)
                throw new SecurityException(Strings.Membership_PasswordMustBeChanged);

            return user;
        }

        /// <summary>
        /// List of current user´s claims
        /// </summary>
        /// <returns></returns>
        public static List<Claim> ListClaims() => ClaimsPrincipal.Current.Identities.First().Claims.ToList();

        /// <summary>
        /// Get current user ID from the claim
        /// </summary>
        /// <returns></returns>
        public static int GetCurrentUserId()
        {
            if (!int.TryParse(GetClaim(ClaimType.UserId), out int currentUserId))
                throw new UnauthorizedAccessException(Strings.Membership_UserNotLogged);

            return currentUserId;
        }

        /// <summary>
        /// Get Claim´s type
        /// </summary>
        /// <param name="claimType"></param>
        /// <returns></returns>
        public static string GetClaim(ClaimType claimType) => ListClaims().FirstOrDefault(x => x.Type.Equals(claimType.ToString()))?.Value;

        /// <summary>
        /// Checks whether the user is authenticated
        /// </summary>
        /// <returns></returns>
        public static bool CurrentUserIsAuthenticated() => HttpContext.Current.User.Identity.IsAuthenticated;

        /// <summary>
        /// Checks if the email entered has already been registered in the database
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static bool CheckIfEmailExist(string email)
            => Functions.CMS.User.Users().Where(u => !string.IsNullOrWhiteSpace(u.Email)).Any(u => u.Email.Equals(email, StringComparison.CurrentCultureIgnoreCase));

        /// <summary>
        /// Based on a dynamic object, create a UserModel
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public static UserModel CreateUserModel(dynamic body, User user = null)
        {
            // Get properties from model
            var modelProperties = typeof(UserModel)
                .GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
                .Select(x => x.Name.ToLower())
                .ToList();

            // User fields that will hold all the custom fields
            var userFields = new List<UserFieldModel>();

            // Convert body into dictionary to be iteracted
            var dictionary = body.ToObject<Dictionary<string, object>>();

            // Remove the properties that belongs to UserModel from field list
            foreach (var item in dictionary)
                if (!modelProperties.Contains(item.Key.ToLower()))
                    userFields.Add(new UserFieldModel { Name = item.Key, Value = (string)item.Value });

            // Create the user model object to store in the database
            return new UserModel
            {
                UserName = (string)body?.userName ?? user?.UserName,
                FirstName = (string)body?.firstName ?? user?.FirstName,
                LastName = (string)body?.lastName ?? user?.LastName,
                Email = (string)body?.email ?? user?.Email,
                Password = (string)body?.password,
                ConfirmPassword = (string)body?.confirmPassword,
                AdminAccess = false,
                Social = body?.social?.ToObject<UserSocialModel>(),
                UserFields = userFields
            };
        }
    }
}