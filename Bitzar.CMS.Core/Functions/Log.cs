using Bitzar.CMS.Data.Model;
using Bitzar.CMS.Extension.CMS;
using MethodCache.Attributes;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bitzar.CMS.Core.Functions.Internal
{
    public class Log : ILog
    {
        /// <summary>
        /// Method to allow user to log exception and record it on the database
        /// </summary>
        /// <param name="objects">Params objects to be added in the log process</param>
        public string LogRequest(params object[] objects)
        {
            return this.LogRequest(null, "Exception", "Bitzar.CMS.Core", objects);
        }

        /// <summary>
        /// Method to allow user to log exception and record it on the database
        /// </summary>
        /// <param name="content">Content for log</param>
        /// <param name="type">Type log</param>
        /// <param name="source">Source log</param>
        /// <param name="objects">Params objects to be added in the log process</param>
        public string LogRequest(object content = null, string type = "Exception", string source = "Bitzar.CMS.Core", params object[] objects)
        {
            try
            {
                // Create the string with data to be returned
                var builder = new StringBuilder();
                if (content != null)
                    builder.AppendLine($"Content:{Environment.NewLine}{JsonConvert.SerializeObject(content)}{Environment.NewLine}{Environment.NewLine}");
                if (objects != null)
                    foreach (var item in objects)
                        builder.AppendLine($"Parameter:{Environment.NewLine}{JsonConvert.SerializeObject(item)}{Environment.NewLine}{Environment.NewLine}");

                // Serialize the string into byte[]
                var name = $"{source}/{type.ToString()}/{DateTime.Now.Date.ToString("yyyy-MM-dd")}/log_{DateTime.Now.ToString("yyyy-MM-ddTHH-mm-ss")}.txt";

                // Upload to blobstorage
                return UploadToAzureStorage(name, builder.ToString());
            }
            catch (Exception e)
            {
                // Ignore
                System.Diagnostics.Trace.WriteLine(e.AllMessages());
                return "";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ReferenceId"></param>
        /// <param name="ReferenceType"></param>
        /// <param name="Source"></param>
        /// <param name="Type"></param>
        /// <param name=""></param>
        /// <param name="Url"></param>
        /// <param name="Description"></param>
        public void SaveLogLink(int ReferenceId, string ReferenceType, string Source, string Type, string Url, string Description = null)
        {
            try
            {
                using (var db = new DatabaseConnection())
                {
                    var log = new LogLink()
                    {
                        ReferenceId = ReferenceId,
                        ReferenceType = ReferenceType,
                        Source = Source,
                        Type = Type,
                        Description = Description,
                        Url = Url
                    };

                    db.LogLinks.Add(log);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.AllMessages());
            }
        }

        /// <summary>
        /// Método de Upload de arquivos no Azure Storage
        /// </summary>
        /// <param name="fileBytes">Archive</param>
        /// <param name="blobName">Name of file</param>
        [NoCache]
        public static string UploadToAzureStorage(string blobName, string content)
        {
            try
            {
                //Checa se a key do storage está configurada
                var azureKey = CMS.Configuration.Get("AzureStorageKey");
                if (string.IsNullOrWhiteSpace(azureKey))
                    throw new System.Exception("Chave do azure storage não configurada.");

                //Checa se o container do storage está configurado
                var azureContainer = CMS.Configuration.Get("AzureStorageContainerForLogs");
                if (string.IsNullOrWhiteSpace(azureContainer))
                    throw new System.Exception("Nome do container não configurado.");

                // Connect to the storage account's blob endpoint 
                var account = CloudStorageAccount.Parse(azureKey);
                var client = account.CreateCloudBlobClient();

                // Create the blob storage container  
                var containerBlob = client.GetContainerReference(azureContainer);
                containerBlob.CreateIfNotExists(BlobContainerPublicAccessType.Blob);

                // Create the blob in the container 
                var blob = containerBlob.GetBlockBlobReference(blobName);

                // Upload the archive and store it in the blob
                blob.UploadText(content, Encoding.UTF8);

                return blob.Uri.AbsoluteUri;
            }
            catch
            {
                throw;
            }
        }

        public static string LoadXml(string ReferenceId, string ReferenceType)
        {
            try
            {
                using (var db = new DatabaseConnection())
                {
                    //Aumenta o limite de tempo fazer uma ação no banco de dados
                    db.Database.CommandTimeout = 120;

                    //Busca o xml ou a url no banco de dados
                    var arq = db.Database.ExecuteSqlCommand("Select").ToString();

                    //Se xml existir executa 
                    if (arq != null)
                        if (arq.Contains("https://"))
                            return DownloadStorage(arq);

                    return "Não existe Url";
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
                var azureKey = CMS.Configuration.Get("AzureStorageKey");
                if (string.IsNullOrWhiteSpace(azureKey))
                    throw new Exception("Chave do azure storage não configurada.");

                // Connect to the storage account's blob endpoint 
                var account = CloudStorageAccount.Parse(azureKey);
                var client = account.CreateCloudBlobClient();

                //Pega o Arquivo
                //Checa se o container do storage está configurado
                var azureContainer = CMS.Configuration.Get("AzureStorageContainer");
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

        /// <summary>
        /// Method to Load all the users that are not disabled
        /// </summary>
        /// <returns></returns>
        public List<Data.Model.LogLink> LogLink()
        {
            using (var db = new DatabaseConnection())
            {
                var source = db.LogLinks.Where(x => x.Url != null).ToList();

                return (from u in source
                        select u).ToList();
            }
        }

        /// <summary>
        /// Method to count the number of logs that exists in the system
        /// </summary>
        /// <param name="admin">Indicates if the users are admin or not</param>
        /// <param name="search">Search parameter to lookup user information</param>
        /// <param name="deepSearch">Indicate that search should be done in user fields</param>
        /// <returns></returns>
        [NoCache]
        public int Count(string search = "")
        {
            // Get user Source
            var source = LogLink();
            var data = new List<Data.Model.LogLink>();

            // Filter data if has filter specified
            if (string.IsNullOrWhiteSpace(search))
                data = source.ToList();
            else
            {
                data.AddRange(source.Where(u => u.Source.ContainsIgnoreCase(search) || u.Type.ContainsIgnoreCase(search) || u.Description.ContainsIgnoreCase(search) || u.ReferenceId.ToString().ContainsIgnoreCase(search) || u.ReferenceType.ContainsIgnoreCase(search)));
            }

            // Return data to show
            return data.DistinctBy(u => u.Id).Count();
        }


        [NoCache]
        public IList<LogLink> List(int page = 1, int size = 25, string search = "")
        {
            // Get user Source
            var source = LogLink();
            var data = new List<LogLink>();

            // Filter data if has filter specified
            if (string.IsNullOrWhiteSpace(search))
                data = source.ToList();
            else
            {
                data.AddRange(source.Where(u => u.Source.ContainsIgnoreCase(search) || u.Type.ContainsIgnoreCase(search) || u.Description.ContainsIgnoreCase(search) || u.ReferenceId == Convert.ToInt32(search) || u.ReferenceType.ContainsIgnoreCase(search)));
            }

            // Return data to show
            return data.DistinctBy(u => u.Id).OrderBy(s => s.Id).Skip((page - 1) * size).Take(size).ToList();
        }
    }
}