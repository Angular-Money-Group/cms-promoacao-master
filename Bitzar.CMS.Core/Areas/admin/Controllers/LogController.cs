using Bitzar.CMS.Core.Helper;
using Bitzar.CMS.Core.Models;
using Bitzar.CMS.Data.Model;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Bitzar.CMS.Core.Areas.admin.Controllers
{
    [RouteArea("Admin", AreaPrefix = "admin")]
    public class LogController : AdminBaseController
    {

        /// <summary>
        /// Default method to show site configuration page
        /// </summary>
        /// <returns></returns>
        [Route("Log/")]
        public ActionResult Index()
        {
            return View();
        }

        [Route("Log/Listar/{pagina?}/{tamanho?}")]
        public ActionResult List( int pagina = 1, int tamanho = 25, string pesquisa = null)
        {
            try
            {
                ViewBag.Search = pesquisa;

                // Set Pagination Objects
                var count = Functions.CMS.Log.Count(pesquisa);
                ViewBag.Pagination = new Pagination()
                {
                    CurrentPage = pagina,
                    Size = tamanho,
                    TotalPages = (int)Math.Ceiling((decimal)count / tamanho),
                    MaxPageItems = 3
                };

                // Get Library data from server
                var data = Functions.CMS.Log.List(pagina, tamanho, pesquisa);

                // Filter user for non-admin access
                if (this.User.Role.Name != "Administrador")
                    data = data.Where(u => u.Id == this.User.Id).ToList();

                return PartialView("_Log", data);
            }
            catch (Exception ex)
            {
                this.NotifyError(ex, ex.AllMessages());
                return PartialView("_Log", null);
            }
        }

        [HttpPost, Route("Log/Carregar-Arquivo")]
        public JsonResult LoadArchive(string ReferenceId, string ReferenceType)
        {
            try
            {
                using (var db = new DatabaseConnection())
                {
                    //Aumenta o limite de tempo fazer uma ação no banco de dados
                    db.Database.CommandTimeout = 120;

                    //Busca o xml ou a url no banco de dados
                    var xml = db.LogLinks.FirstOrDefault(f => f.ReferenceId.ToString() == ReferenceId && f.ReferenceType == ReferenceType);



                    if (xml != null)
                        if (xml.Url.Contains("https://"))
                        {
                            var arq = DownloadStorage(xml?.Url);

                            arq = arq.Replace("Content:", "");

                            var js = JsonConvert.DeserializeObject(arq).ToString();

                            return Json(new JsonResponse() { Result = js }, JsonRequestBehavior.AllowGet);
                        }
                        else
                            return Json(new JsonResponse() { Result = "Não existe Url" }, JsonRequestBehavior.AllowGet);
                    else
                        return Json(new JsonResponse() { Result = "Log não cadastrado" }, JsonRequestBehavior.AllowGet);

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string DownloadStorage(string url)
        {
            try
            {
                //Checa se a key do storage está configurada
                var azureKey = Functions.CMS.Configuration.Get("AzureStorageKey");
                if (string.IsNullOrWhiteSpace(azureKey))
                    throw new Exception("Chave do azure storage não configurada.");

                // Connect to the storage account's blob endpoint 
                var account = CloudStorageAccount.Parse(azureKey);
                var client = account.CreateCloudBlobClient();

                //Pega o Arquivo
                //Checa se o container do storage está configurado
                var azureContainer = "log";
                if (string.IsNullOrWhiteSpace(azureContainer))
                    throw new System.Exception("Nome do container não configurado.");

                var containerBlob = client.GetContainerReference(azureContainer);

                var urlBlob = $"{containerBlob.StorageUri.PrimaryUri.ToString()}/";
                var name = url.Replace(urlBlob, "");
                var blob = containerBlob.GetBlockBlobReference(name);

                return blob.DownloadText();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}