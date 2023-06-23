using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitzar.CMS.Extension.CMS
{
    public interface ISecurity
    {
        string RequestToken { get; }
        bool ValidateToken(string token);
        string Encrypt(string text);
        string Decrypt(string cipher);
    }
}
