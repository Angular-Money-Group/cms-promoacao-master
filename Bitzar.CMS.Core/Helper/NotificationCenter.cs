using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Bitzar.CMS.Core.Helper
{
    public static class NotificationCenter
    {
        public static List<Extension.Classes.Notification> SystemNotification(UrlHelper url)
        {
            var notifications = new List<Extension.Classes.Notification>();

            if (UpdateHelper.CheckNewVersion().Any())
            {
                notifications.Add(new Bitzar.CMS.Extension.Classes.Notification()
                {
                    Title = "Atualização Disponível",
                    Badge = 1,
                    Description = "Clique aqui para atualizar...",
                    Icon = "wb-alert",
                    UrlFunction = url.Action("Index", "Default", new { area = "update" })
                });
            }

            return notifications;
        }
    }
}