using Bitzar.CMS.Core.Functions;
using Bitzar.CMS.Core.Resources;
using Bitzar.CMS.Data;
using Microsoft.Owin.Security.OAuth;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace Bitzar.CMS.Core.Configurations
{
    public class SimpleAuthorizationServerProvider : OAuthAuthorizationServerProvider
    {
        public const string OwinChallenge = "x-challenge";

        /// <summary>
        /// Internal method to validate the client authentication
        /// </summary>
        /// <param name="context">Context provided</param>
        /// <returns></returns>
        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context) => await Task.Run(() => context.Validated());

        /// <summary>
        /// Create the authentication context provided by the client with user and password.
        /// Will be responsible to add all the necessary claims in the identity service
        /// </summary>
        /// <param name="context">Context provided</param>
        /// <returns></returns>
        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            await Task.Run(() =>
            {
                context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" });

                try
                {
                    // Check if membership is enabled
                    if (!Functions.CMS.Configuration.Get("MembershipEnabled").Contains("true"))
                        throw new NotSupportedException(Strings.Membership_MustBeEnabledToUseApi);

                    // Validate the input if it's ok
                    if (string.IsNullOrWhiteSpace(context.UserName) || string.IsNullOrWhiteSpace(context.Password))
                        throw new Exception(Strings.Membership_UserNameAndPasswordMustBeProvided);

                    // Execute the login service
                    var user = Authentication.Login(context.UserName, context.Password)
                        ?? throw new UnauthorizedAccessException(Strings.Authentication_UserNotFound);

                    // Create the identity claims object and insert data on it
                    var identity = new ClaimsIdentity(context.Options.AuthenticationType);

                    identity.AddClaim(new Claim(ClaimType.Mail.ToString(), user.Email ?? string.Empty));
                    identity.AddClaim(new Claim(ClaimType.Role.ToString(), user.Role?.Name ?? string.Empty));
                    identity.AddClaim(new Claim(ClaimType.LastName.ToString(), user.LastName ?? string.Empty));
                    identity.AddClaim(new Claim(ClaimType.UserName.ToString(), user.UserName ?? string.Empty));
                    identity.AddClaim(new Claim(ClaimType.FirstName.ToString(), user.FirstName ?? string.Empty));
                    identity.AddClaim(new Claim(ClaimType.UserId.ToString(), user.Id.ToString() ?? string.Empty));
                    identity.AddClaim(new Claim(ClaimType.RoleId.ToString(), user.Role?.Id.ToString() ?? string.Empty));
                    identity.AddClaim(new Claim(ClaimType.System.ToString(), user.AdminAccess.ToString() ?? string.Empty));

                    // Set the current principal
                    var principal = new GenericPrincipal(identity, new string[] { user.AdminAccess ? "admin" : "" });
                    Thread.CurrentPrincipal = principal;

                    context.Validated(identity);
                }
                catch (NotSupportedException nsException)
                {
                    context.Response.Headers.Add(OwinChallenge, new[] { ((int)HttpStatusCode.Conflict).ToString() });
                    context.SetError(nsException.Message);
                }
                catch (UnauthorizedAccessException uaException)
                {
                    context.Response.Headers.Add(OwinChallenge, new[] { ((int)HttpStatusCode.Forbidden).ToString() });
                    context.SetError(uaException.Message);
                }
                catch (ValidationException dataException)
                {
                    context.Response.Headers.Add(OwinChallenge, new[] { ((int)HttpStatusCode.NotAcceptable).ToString() });
                    context.SetError(dataException.Message);
                }
                catch (Exception exception)
                {
                    context.SetError(exception.Message);
                }
            });
        }
    }
}