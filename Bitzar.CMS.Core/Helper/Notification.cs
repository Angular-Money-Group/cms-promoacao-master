using Bitzar.CMS.Core.Functions.Internal;
using Bitzar.CMS.Core.Helper;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;

public class Notification
{
    public const string NOTIFICATION = "NOTIFICATION";
    public enum Type { SUCCESS, INFO, WARNING, DANGER }

    public string Title { get; set; }
    public string Message { get; set; }
    public Type NotificationType { get; set; }
    public bool Toast { get; set; } = true;

    public Notification(string title, string message, Type type, bool toast)
    {
        this.Title = title.Replace("'", "\"").Replace(Environment.NewLine, "<br/>");
        this.Message = message.Replace("'", "\"").Replace(Environment.NewLine, "<br/>");
        this.NotificationType = type;
        this.Toast = toast;
    }

    public string ShowNotification(bool forceAlert = false)
    {
        // Show the toast notification
        if (this.Toast && !forceAlert)
        {
            var type = this.NotificationType.ToString().ToLower();
            if (type == "danger")
                type = "error";

            if (type != "error")
                return string.Format("toastr.{0}('{2}','{1}');", type, this.Title, this.Message);
            else
                return string.Format("toastr.{0}('{2}','{1}', {{ timeOut : 0}});", type, this.Title, this.Message);
        }

        // Show alert notification
        return string.Format("<div id=\"notify\" class=\"alert alert-{0} dark fade in margin-top-20 margin-bottom-0\">" +
        "<a href=\"#\" class=\"close\" data-dismiss=\"alert\" aria-label=\"close\">&times;</a>" +
        "<strong>{1}</strong> {2}</div>", this.NotificationType.ToString().ToLower(), this.Title, this.Message);
    }
}

public static class NotificationExtension
{

    public static void NotifySuccess(this Controller context, string message, string title = "Sucesso", bool toast = true)
    {
        context.TempData[Notification.NOTIFICATION] = new Notification(title, message, Notification.Type.SUCCESS, toast);
    }

    public static void NotifyInfo(this Controller context, string message, string title = "Info", bool toast = true)
    {
        context.TempData[Notification.NOTIFICATION] = new Notification(title, message, Notification.Type.INFO, toast);
    }

    public static void NotifyWarning(this Controller context, string message, string title = "Atenção", bool toast = true)
    {
        context.TempData[Notification.NOTIFICATION] = new Notification(title, message, Notification.Type.WARNING, toast);
    }

    public static void NotifyError(this Controller context, Exception ex, string message, string title = "Erro", bool toast = true)
    {
        context.TempData[Notification.NOTIFICATION] = new Notification(title, message, Notification.Type.DANGER, toast);
        // Log Exception
        Log log = new Log();
        var parameters = new
        {
            Context = context,
            Exception = ex
        };
        
        log.LogRequest(parameters);
    }

    public static bool HasPendingNotification(this Controller context)
    {
        return context.TempData[Notification.NOTIFICATION] != null;
    }

    public static void ClearPendingNotification(this Controller context)
    {
        context.TempData[Notification.NOTIFICATION] = null;
    }
}