using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace Bitzar.CMS.Core.Helper.ViewFromAssembly
{
    public class CmsVirtualFile : VirtualFile
    {
        private byte[] viewContent;

        public CmsVirtualFile(string virtualPath, byte[] viewContent) : base(virtualPath)
        {
            this.viewContent = viewContent;
        }

        public override Stream Open()
        {
            return new MemoryStream(viewContent);
        }
    }
}