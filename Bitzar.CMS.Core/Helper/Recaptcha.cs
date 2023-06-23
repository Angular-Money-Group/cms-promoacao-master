using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Script.Serialization;

namespace Bitzar.CMS.Core.Helper
{
    /// <summary>
    /// Helper Methods for the Google Recaptcha V2 Library
    /// </summary>
    public class Recaptcha
    {
        /// <summary>
        /// Validates a Recaptcha V2 response.
        /// </summary>
        /// <param name="captcha">g-recaptcha-response form response variable (HttpContext.Current.Request.Form["g-recaptcha-response"])</param>
        /// <returns>RecaptchaValidationResult</returns>
        public static RecaptchaValidationResult Validate(string site, string secretKey, string captcha, string ip)
        {
            // Create request
            var webRequest = (HttpWebRequest)WebRequest.Create($"{site}?secret={secretKey}&response={captcha}&remoteip={ip}");

            //Google recaptcha Response
            using (WebResponse response = webRequest.GetResponse())
            using (StreamReader content = new StreamReader(response.GetResponseStream()))
            {
                string jsonResponse = content.ReadToEnd();
                return (new JavaScriptSerializer()).Deserialize<RecaptchaValidationResult>(jsonResponse.Replace("error-codes", "ErrorMessages").Replace("success", "Succeeded"));
            }
        }

        public class RecaptchaValidationResult
        {
            public RecaptchaValidationResult()
            {
                ErrorMessages = new List<string>();
                Succeeded = false;
            }

            public List<string> ErrorMessages { get; set; }
            public bool Succeeded { get; set; }

            public string GetErrorMessagesString()
            {
                return string.Join("<br/>", ErrorMessages.ToArray());
            }
        }
    }
}