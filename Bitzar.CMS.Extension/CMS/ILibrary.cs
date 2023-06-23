using Bitzar.CMS.Data.Model;
using System.Collections.Generic;
using System.IO;
using System.Web;

namespace Bitzar.CMS.Extension.CMS
{
    public interface ILibrary
    {
        Library Object(int id);
        Library Object(string fileName);
        IList<Library> UploadFiles(HttpFileCollectionBase files);
        byte[] OptimizeImage(Stream fileData, bool isImageType, string extension);
        void DeleteAzureStorage(string blobName);
        void RenameAzureStorage(string blobName, string newName);
        string UploadToAzureStorage(byte[] fileBytes, string blobName);
        string UploadToAzureStorage(Stream stream, string blobName);
    }
}