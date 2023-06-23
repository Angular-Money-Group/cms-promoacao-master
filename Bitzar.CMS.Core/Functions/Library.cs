using Bitzar.CMS.Core.Helper;
using Bitzar.CMS.Data.Model;
using Bitzar.CMS.Extension.CMS;
using MethodCache.Attributes;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using TinifyAPI;

namespace Bitzar.CMS.Core.Functions.Internal
{
    /// <summary>
    /// Class to hold and organize library functions
    /// </summary>
    [Cache(Members.All)]
    public class Library : Cacheable, ILibrary
    {
        #region Internal Methods that are not exposed to layout system
        /// <summary>
        /// Method to return the Library Types that exists on the database
        /// </summary>
        internal List<LibraryType> Types()
        {
            using (var db = new DatabaseConnection())
                return db.LibraryTypes.ToList();
        }

        /// <summary>
        /// Method to cache all available Media
        /// </summary>
        /// <returns></returns>
        internal List<Data.Model.Library> Objects()
        {
            using (var db = new DatabaseConnection())
            {
                var objectList = db.Library.Include(t => t.LibraryType).ToList();

                // Check if should use azure blob storage
                if (!CMS.Configuration.Get("AzureStorage").Contains("true"))
                    return objectList;

                // Get azure url information
                var urlAzure = CMS.Configuration.Get("AzureStorageUrl");
                if (string.IsNullOrWhiteSpace(urlAzure))
                    return objectList;

                var container = CMS.Configuration.Get("AzureStorageContainer");
                if (string.IsNullOrWhiteSpace(container))
                    return objectList;

                // Bind Azure Url
                var path = $"{urlAzure}/{container}".Replace("//","/").Replace(":/", "://");
                objectList.ForEach(o => o.Path = $"{path}/{o.Path}/".Replace("/~/","/").TrimEnd('/'));

                return objectList;
            }
        }

        /// <summary>
        /// Method to return the Library Content that exists on the database
        /// </summary>
        [NoCache]
        internal List<Data.Model.Library> Media(int page = 1, int pageSize = 50, string type = null, string filter = null)
        {
            using (var db = new DatabaseConnection())
            {
                if (type != "midia")
                    return (from m in Objects()
                            where (type == null || m.LibraryType.Description.Equals(type, StringComparison.CurrentCultureIgnoreCase)) &&
                                    (filter == null || m.Name.ToLower().Contains(filter) || (m.Description?.ToLower().Contains(filter) ?? false))
                            orderby m.CreatedAt descending
                            select m).Skip((page - 1) * pageSize).Take(pageSize).ToList();
                else
                    return (from m in Objects()
                            where (type == null || m.LibraryType.Description.Equals("Image", StringComparison.CurrentCultureIgnoreCase) ||
                            m.LibraryType.Description.Equals("Audio", StringComparison.CurrentCultureIgnoreCase) ||
                            m.LibraryType.Description.Equals("Video", StringComparison.CurrentCultureIgnoreCase) ||
                            m.LibraryType.Description.Equals("Other", StringComparison.CurrentCultureIgnoreCase)) &&
                            (filter == null || m.Name.ToLower().Contains(filter) || (m.Description?.ToLower().Contains(filter) ?? false))
                            orderby m.CreatedAt descending
                            select m).Skip((page - 1) * pageSize).Take(pageSize).ToList();
            }
        }

        /// <summary>
        /// Method to return the Library Content that exists on the database
        /// </summary>
        [NoCache]
        internal int MediaCount(string type = null, string filter = null)
        {
            filter = filter?.ToLower();
            using (var db = new DatabaseConnection())
                return (from m in Objects()
                        where (type == null || m.LibraryType.Description.Equals(type, StringComparison.CurrentCultureIgnoreCase)) &&
                                (filter == null || m.Name.ToLower().Contains(filter) || (m.Description?.ToLower().Contains(filter) ?? false))
                        orderby m.CreatedAt descending
                        select m).Count();
        }

        /// <summary>
        /// Method to return to the system the allowed MimeTypes to upload
        /// </summary>
        internal string[] AllowedMimeTypes
        {
            get => Types().Select(t => t.MimeTypes).ToArray();
        }
        #endregion

        /// <summary>
        /// Method to Load an Library Object from the system
        /// </summary>
        /// <param name="fileName">Image file Name</param>
        /// <returns>Returns the relative directory name of the fileName</returns>
        [NoCache]
        public Data.Model.Library Object(string fileName)
        {
            return Objects().FirstOrDefault(f => f.Name == fileName);
        }

        /// <summary>
        /// Method to Load an Library Object from the system
        /// </summary>
        /// <param name="id">Id of the Object</param>
        /// <returns>Returns the relative directory name of the fileName</returns>
        [NoCache]
        public Data.Model.Library Object(int id)
        {
            return Objects().FirstOrDefault(f => f.Id == id);
        }

        /// <summary>
        /// Method to upload a set of files to the system and return all of them in an array
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        [NoCache]
        public IList<Data.Model.Library> UploadFiles(HttpFileCollectionBase files)
        {
            try
            {
                // Check if there is any file to send
                if (files.Count == 0)
                    throw new InvalidOperationException(Resources.Strings.Template_MustSendFile);

                // Check extension to see if there is any problem
                var types = CMS.Library.Types();
                var allowedExtensions = string.Join(",", types.Select(t => t.AllowedExtensions)).Replace(" ", "").Split(',');
                var uploadedFiles = files.GetMultiple("FileUpload").Where(t =>  allowedExtensions.Any(f => f.Equals(System.IO.Path.GetExtension(t.FileName), StringComparison.CurrentCultureIgnoreCase)));
                if (uploadedFiles.Count() == 0)
                    throw new InvalidOperationException(Resources.Strings.Library_ExtensionNotAllowed);

                // All Files are allowed, so keep going and save each of them
                var list = new List<Data.Model.Library>();
                using (var db = new DatabaseConnection())
                {
                    foreach (HttpPostedFileBase file in uploadedFiles)
                    {
                        // Set File Name if already exists
                        var fileName = file.FileName.Replace(" ", "_").ClearInvalidChars();
                        if (db.Library.Any(f => f.Name == fileName))
                            fileName = $"{System.IO.Path.GetFileNameWithoutExtension(fileName)}_{DateTime.Now:mmssfff}{System.IO.Path.GetExtension(fileName)}";
                        
                        // Locate mimetype
                        var type = types.FirstOrDefault(t => t.AllowedExtensions.ToLower().Contains(System.IO.Path.GetExtension(fileName).ToLower()));

                        // Create library item
                        var item = new Data.Model.Library()
                        {
                            Name = fileName,
                            CreatedAt = DateTime.Now,
                            Extension = System.IO.Path.GetExtension(fileName),
                            IdLibraryType = type.Id,
                            Path = type.DefaultPath,
                        };

                        // Get image Width and Height
                        if (type.IsImageType && item.Extension != ".webp")
                            using (var img = Image.FromStream(file.InputStream))
                                item.Attributes = $"{img.Width}x{img.Height}";

                        //Compress image with TinyPNG API
                        var fileBytes = OptimizeImage(file.InputStream, type.IsImageType, item.Extension);
                        item.Size = fileBytes.Length;

                        // Add the new template in the databse
                        db.Library.Add(item);
                        list.Add(item);

                        if (CMS.Configuration.Get("AzureStorage").Contains("true"))
                        {
                            var blobName = $"{type.DefaultPath}/{fileName}".Replace("~/", "");
                            UploadToAzureStorage(fileBytes, blobName);
                        }
                        else
                        {
                            // Save the new File on Disk
                            var name = System.IO.Path.Combine(HostingEnvironment.MapPath(type.DefaultPath), fileName);
                            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(name));

                            File.WriteAllBytes(name, fileBytes);
                        }
                    }

                    // Apply changes on the database
                    db.SaveChanges();

                    // Clear Cache
                    CMS.ClearCache(typeof(Internal.Library).FullName);

                    // Return the list of files to the system
                    return list;
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Método de migrar todos os arquivos da biblioteca para o Azure Storage
        /// </summary>
        [NoCache]
        public void MigrateLybraryToAzureStorage()
        {
            //Carrega chaves de configuração do Azure Storage
            var azureKey = CMS.Configuration.Get("AzureStorageKey");
            var azureContainer = CMS.Configuration.Get("AzureStorageContainer");

            if (!CMS.Configuration.Get("AzureStorage").Contains("true"))
                return;

            // Connect to the storage account's blob endpoint 
            var account = CloudStorageAccount.Parse(azureKey);
            var client = account.CreateCloudBlobClient();

            // Create the blob storage container
            var containerBlob = client.GetContainerReference(azureContainer);
            containerBlob.CreateIfNotExists(BlobContainerPublicAccessType.Blob);

            foreach(var item in Objects())
            {
                var file = System.IO.Path.Combine(HostingEnvironment.MapPath(item.LibraryType.DefaultPath), item.Name);
                if (!File.Exists(file))
                    continue;

                //Carrega a referencia do blob
                var blobName = $"{item.LibraryType.DefaultPath}/{item.Name}".Replace("~/", "");
                var blob = containerBlob.GetBlockBlobReference(blobName);
                blob.DeleteIfExists();

                using(var fileStream = new FileStream(file, FileMode.Open))
                    blob.UploadFromStream(fileStream);

                File.Delete(file);
            }
        }

        /// <summary>
        /// Método de renomear arquivo no AzureStorage
        /// </summary>
        /// <param name="blobName"></param>
        /// <param name="newName"></param>
        [NoCache]
        public void RenameAzureStorage(string blobName, string newName)
        {
            try
            {
                //Carrega chaves de configuração do Azure Storage
                var azureKey = CMS.Configuration.Get("AzureStorageKey");
                var azureUrl = CMS.Configuration.Get("AzureStorageUrl");
                var azureContainer = CMS.Configuration.Get("AzureStorageContainer");

                // Connect to the storage account's blob endpoint 
                var account = CloudStorageAccount.Parse(azureKey);
                var client = account.CreateCloudBlobClient();

                // Create the blob storage container
                var containerBlob = client.GetContainerReference(azureContainer);
                containerBlob.CreateIfNotExists(BlobContainerPublicAccessType.Blob);

                //Carrega a referencia do blob
                var blob = containerBlob.GetBlockBlobReference(blobName);
                blob.FetchAttributes();

                //Faz o download do arquivo
                var buffer = new byte[blob.Properties.Length];
                blob.DownloadToByteArray(buffer, 0);

                //Cria uma nova referencia do blob
                var blobRenamed = containerBlob.GetBlockBlobReference(newName);

                //Salva o arquivo dentro do blob novo
                blobRenamed.UploadFromByteArray(buffer, 0, buffer.Length);
                
                //Deleta o Arquivo com o Nome antigo
                blob.DeleteIfExists();
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Método de deletar arquivos do Azure Storage
        /// </summary>
        /// <param name="blobName">Name of file</param>
        /// <param name="newName">New Name of file</param>
        [NoCache]
        public void DeleteAzureStorage(string blobName)
        {
            try
            {
                //Variaveis de configuração do Azure Storage
                var azureKey = CMS.Configuration.Get("AzureStorageKey");
                var azureUrl = CMS.Configuration.Get("AzureStorageUrl");
                var azureContainer = CMS.Configuration.Get("AzureStorageContainer");

                // Connect to the storage account's blob endpoint 
                var account = CloudStorageAccount.Parse(azureKey);
                var client = account.CreateCloudBlobClient();

                // Create the blob storage container
                var containerBlob = client.GetContainerReference(azureContainer);
                containerBlob.CreateIfNotExists(BlobContainerPublicAccessType.Blob);

                //Delete the archive in the blob
                containerBlob.GetBlockBlobReference(blobName).DeleteIfExists();
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Create the blob reference from the storage key and container for the blob name
        /// </summary>
        /// <param name="blobName">Blob name to create or get referece</param>
        /// <returns>returns an instance to the block blob</returns>
        private static CloudBlockBlob GetBlobReference(string blobName)
        {
            //Checa se a key do storage está configurada
            var azureKey = CMS.Configuration.Get("AzureStorageKey");
            if (string.IsNullOrWhiteSpace(azureKey))
                throw new System.Exception("Chave do azure storage não configurada.");

            //Chega se a url do storage está configurada
            var azureUrl = CMS.Configuration.Get("AzureStorageUrl");
            if (string.IsNullOrWhiteSpace(azureUrl))
                throw new System.Exception("Url do azure storage não configurada.");

            //Checa se o container do storage está configurado
            var azureContainer = CMS.Configuration.Get("AzureStorageContainer");
            if (string.IsNullOrWhiteSpace(azureContainer))
                throw new System.Exception("Nome do container não configurado.");

            // Connect to the storage account's blob endpoint 
            var account = CloudStorageAccount.Parse(azureKey);
            var client = account.CreateCloudBlobClient();

            // Create the blob storage container  
            var containerBlob = client.GetContainerReference(azureContainer);
            containerBlob.CreateIfNotExists(BlobContainerPublicAccessType.Blob);

            // Create the blob in the container 
            return containerBlob.GetBlockBlobReference(blobName);
        }

        /// <summary>
        /// Método de Upload de arquivos no Azure Storage
        /// </summary>
        /// <param name="fileBytes">Archive</param>
        /// <param name="blobName">Name of file</param>
        [NoCache]
        public string UploadToAzureStorage(Stream stream, string blobName)
        {
            try
            {
                var blob = GetBlobReference(blobName);

                // Upload the archive and store it in the blob
                blob.UploadFromStream(stream);

                return blob.Uri.AbsoluteUri;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Método de Upload de arquivos no Azure Storage
        /// </summary>
        /// <param name="fileBytes">Archive</param>
        /// <param name="blobName">Name of file</param>
        [NoCache]
        public string UploadToAzureStorage(byte[] fileBytes, string blobName)
        {
            try
            {
                var blob = GetBlobReference(blobName);

                // Upload the archive and store it in the blob
                blob.UploadFromByteArray(fileBytes, 0, fileBytes.Length);

                return blob.Uri.AbsoluteUri;
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Method to compress image with TinifyAPI
        /// </summary>
        /// <param name="fileData">Image file Name </param>
        /// <param name="isImageType">Image file Type</param>
        /// <param name="extension">Image Extension</param>
        /// <returns></returns>
        [NoCache]
        public byte[] OptimizeImage(Stream fileData, bool isImageType, string extension)
        {
            //Create byteArray
            byte[] sourceData = null;
            using (MemoryStream ms = new MemoryStream())
            {
                fileData.Position = 0;
                fileData.CopyTo(ms);
                sourceData = ms.ToArray();
            }
            
            //Check if obey all parameters and conditions
            var optimizeImg = CMS.Configuration.Get("OptimizeImages").Contains("true");
            var optimizeToken = CMS.Configuration.Get("OptimizeImagesToken");
            if (!isImageType || extension == ".gif" || !optimizeImg || string.IsNullOrWhiteSpace(optimizeToken))
                return sourceData;
            
            return Task.Run(async () =>
            {
                try
                {
                    //Check and validate Token
                    Tinify.Key = optimizeToken;
                    if (!(await Tinify.Validate()) || Tinify.CompressionCount >= 500)
                        return sourceData;
                    
                    return await Tinify.FromBuffer(sourceData).ToBuffer();
                }
                catch (System.Exception e)
                {
                    Debug.WriteLine(e.Message);
                    //Return if occur exception
                    return sourceData;
                }
            }).Result;
        }
    }
}

