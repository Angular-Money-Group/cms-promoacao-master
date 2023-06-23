using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Bitzar.CMS.Core.Areas.api.Models
{
    public class UserModel
    {
        [JsonProperty("id")]
        public int? Id { get; set; }
        [JsonProperty("RoleId")]
        public int? RoleId { get; set; }
        [JsonProperty("userName")]
        public string UserName { get; set; }
        [JsonProperty("firstName")]
        public string FirstName { get; set; }
        [JsonProperty("lastName")]
        public string LastName { get; set; }
        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("password")]
        public string Password { get; set; }
        [JsonProperty("confirmPassword")]
        public string ConfirmPassword { get; set; }
        [JsonProperty("profilePicture")]
        public string ProfilePicture { get; set; }
        [JsonProperty("sendMail")]
        public bool SendMail { get; set; }
        [JsonProperty("token")]
        public string Token { get; set; }
        [JsonProperty("validated")]
        public DateTime? Validated { get; set; }
        [JsonProperty("adminAccess")]
        public bool AdminAccess { get; set; }
        [JsonProperty("social")]
        public UserSocialModel Social { get; set; }
        [JsonProperty("userFields")]
        public List<UserFieldModel> UserFields { get; set; }

        internal void Create(int? id, int? roleId, string userName, string firstName, string lastName, string email, string password, 
                           string confirmPassword, string profilePicture, string token, DateTime? validated, 
                           bool adminAccess, UserSocialModel social, List<UserFieldModel> userFields)
        {
            Id = id;
            RoleId = roleId;
            UserName = userName;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Password = password;
            ConfirmPassword = confirmPassword;
            ProfilePicture = profilePicture;
            Token = token;
            Validated = validated;
            AdminAccess = adminAccess;
            Social = social;
            UserFields = userFields;
        }
    }
}