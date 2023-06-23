using Bitzar.CMS.Core.Areas.api.Helpers;
using Bitzar.CMS.Core.Areas.api.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Bitzar.CMS.Core.Areas.api.Controllers
{
    [RoutePrefix("api/v1")]
    public class CallbackController : BaseController
    {

        [Route("callback/braspag")]
        [AllowAnonymous]
        [HttpPost]
        public async Task<HttpResponseMessage> Callback([FromBody] NotificationModel notificationModel)
        {
            try
            {
                dynamic result = string.Empty;

                var log = new Functions.Internal.Log();

                log.LogRequest(notificationModel, "Info", "Bitzar.CMS.Core");

                if (notificationModel.ChangeType != null && notificationModel.PaymentId != null)
                {
                    if (notificationModel.ChangeType.Equals("1"))
                    {
                        log.LogRequest("Callback reconhecido e enviando apra o módulo de pagamento", "Info", "Bitzar.CMS.Core");

                        var parameters = new Dictionary<string, string>();
                        parameters.Add("platform", "Braspag");
                        parameters.Add("paymentId", notificationModel.PaymentId);

                        result = PluginHelpers.Execute("Bitzar.Payments.dll", "Callback", "bb835fcc-8caa-4f3c-bfda-828501d0ac24", parameters);

                    }
                }

                return await CreateResponse(result);
            }
            catch (Exception ex)
            {
                return await this.HandleException(ex);
            }
        }
    }
}