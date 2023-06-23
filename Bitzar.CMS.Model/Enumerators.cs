using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitzar.CMS.Model
{
    public class Enumerators
    {
        public enum EventType
        {
            PreValidateExecute = -1,
            OnSaveContent = 0,
            OnSaveUser = 1,
            OnLogin = 2,
            OnSaveConfiguration = 3,
            OnSaveLanguage = 4,
            OnUploadPlugin = 5,
            OnSaveTemplate = 6,
            OnNotification = 7
        }
    }
}
