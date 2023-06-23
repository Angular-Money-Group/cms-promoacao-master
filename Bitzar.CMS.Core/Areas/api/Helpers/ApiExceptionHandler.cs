using System;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using System.Security;
using System.Threading.Tasks;

namespace Bitzar.CMS.Core.Areas.api.Helpers
{
    public static class ApiExceptionHandler
    {
        public static async Task<HttpResponseMessage> HandleException(this BaseController controller, Exception exception, object data = null, [CallerMemberName] string caller = null)
        {
            // Log the exception in error handling
            LogHelper.Register(exception, controller.ControllerContext, caller, data);

            // Get the reference status code and return data
            return await controller.CreateErrorResponse(GetExceptionStatusCode(exception), exception.Message, data);
        }

        /// <summary>
        /// Translate the exception type for the status code
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        private static HttpStatusCode GetExceptionStatusCode(Exception exception)
        {
            // Handle exception for not authorized information - 403
            if (exception is UnauthorizedAccessException)
                return HttpStatusCode.Forbidden;

            // Handle exception for arguments that is invalid - 400
            if (exception is ArgumentException || exception is ArgumentNullException)
                return HttpStatusCode.BadRequest;

            // Handle exception for invalidoperation - 405
            if (exception is InvalidOperationException || exception is InvalidDataException)
                return HttpStatusCode.MethodNotAllowed;

            // Handle exception for not found objects - 417
            if (exception is ObjectNotFoundException)
                return HttpStatusCode.ExpectationFailed;

            // Handle exceptions for configuration errors - 412
            if (exception is ConfigurationErrorsException)
                return HttpStatusCode.PreconditionFailed;

            // Handle the not suported operations - 409
            if (exception is NotSupportedException)
                return HttpStatusCode.Conflict;

            // Handle exception for validation - 406
            if (exception is ValidationException)
                return HttpStatusCode.NotAcceptable;

            // Handle exceptions for External Services (Email/Database/other) - 502
            if (exception is SmtpException || exception is DBConcurrencyException || exception is DbUpdateConcurrencyException)
                return HttpStatusCode.BadGateway;

            // handle exception for need to change passwod - 426
            if (exception is SecurityException)
                return HttpStatusCode.UpgradeRequired;

            // Deafault status code
            return HttpStatusCode.BadRequest;
        }
    }
}