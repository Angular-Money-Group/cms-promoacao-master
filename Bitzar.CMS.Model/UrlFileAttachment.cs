using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace Bitzar.CMS.Model
{
    public class UrlFileAttachment
    {
        public string FileName { get; set; }
        public string Url { get; set; }

        public Stream Stream
        {
            get
            {
                var request = (HttpWebRequest)WebRequest.Create(this.Url);
                var response = (HttpWebResponse)request.GetResponse();

                return response.GetResponseStream();
            }
        }
    }
}