using Bitzar.CMS.Core.Functions;
using Bitzar.CMS.Core.Helper;
using Bitzar.CMS.Core.Models;
using Bitzar.CMS.Data.Model;
using Bitzar.CMS.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.SessionState;

public static class Extensions
{
    #region Session extensions to help the system.
    /// <summary>
    /// Função utilizada para recuperar um objeto armazenado na sessão
    /// </summary>
    /// <typeparam name="T">Tipo do objeto a ser recuperad</typeparam>
    /// <param name="session">Sessão</param>
    /// <returns>Retorna o objeto recuperado da sessão</returns>
    public static T Get<T>(this HttpSessionStateBase session, string key = null)
    {
        // Validate if the session is invalid
        if (session == null)
        {
            FormsAuthentication.RedirectToLoginPage();
            return default(T);
        }

        // Load name of the type to be stored
        if (String.IsNullOrWhiteSpace(key))
            key = Extensions.NameOfType<T>();

        // if not found at the session, should return default(T) = null
        if (session[key] == null)
            return default(T);

        return (T)session[key];
    }

    /// <summary>
    /// Função utilizada para armazenar um objeto na sessão
    /// </summary>
    /// <typeparam name="T">Tipo do objeto a ser armazenado na sessão</typeparam>
    /// <param name="session">Sessão</param>
    /// <param name="obj">objeto a ser armazenado na sessão</param>
    public static void Set<T>(this HttpSessionStateBase session, T obj, string key = null)
    {
        // Validate if the session is invalid
        if (session == null)
            FormsAuthentication.RedirectToLoginPage();

        // Load name of the type to be stored
        if (String.IsNullOrWhiteSpace(key))
            key = Extensions.NameOfType<T>();

        session[key] = obj;
    }

    /// <summary>
    /// Função utilizada para recuperar um objeto armazenado na sessão
    /// </summary>
    /// <typeparam name="T">Tipo do objeto a ser recuperad</typeparam>
    /// <param name="session">Sessão</param>
    /// <returns>Retorna o objeto recuperado da sessão</returns>
    public static T Get<T>(this HttpSessionState session, string key = null)
    {
        // Validate if the session is invalid
        if (session == null)
        {
            FormsAuthentication.RedirectToLoginPage();
            return default(T);
        }

        // Load name of the type to be stored
        if (String.IsNullOrWhiteSpace(key))
            key = Extensions.NameOfType<T>();

        // if not found at the session, should return default(T) = null
        if (session[key] == null)
            return default(T);

        return (T)session[key];
    }

    /// <summary>
    /// Função utilizada para armazenar um objeto na sessão
    /// </summary>
    /// <typeparam name="T">Tipo do objeto a ser armazenado na sessão</typeparam>
    /// <param name="session">Sessão</param>
    /// <param name="obj">objeto a ser armazenado na sessão</param>
    public static void Set<T>(this HttpSessionState session, T obj, string key = null)
    {
        // Validate if the session is invalid
        if (session == null)
            FormsAuthentication.RedirectToLoginPage();

        // Load name of the type to be stored
        if (String.IsNullOrWhiteSpace(key))
            key = Extensions.NameOfType<T>();

        session[key] = obj;
    }

    /// <summary>
    /// Função para criar um nome genérico para ser armazenado na sessão
    /// </summary>
    /// <typeparam name="T">Tipo genérico que será armazenado na sessão</typeparam>
    /// <returns>Retorna uma string formatada com o nome gerado</returns>
    internal static string NameOfType<T>()
    {
        return string.Format("BTZ#{0}", typeof(T).Name);
    }

    /// <summary>
    /// Static method to recursevely returns all the exception messages that have been throwned
    /// </summary>
    /// <param name="exception">Source exception to get</param>
    /// <returns>Returns a list o messages</returns>
    public static string AllMessages(this Exception exception)
    {
        var list = new List<string>();
        if (exception is DbEntityValidationException)
        {
            var ex = (DbEntityValidationException)exception;
            list.Add(ex.Message);
            foreach (var validation in ex.EntityValidationErrors.Where(v => !v.IsValid))
            {
                list.AddRange(validation.ValidationErrors.Select(e => e.ErrorMessage).ToList());
            }
        }
        else
        {
            var ex = exception;
            do
            {
                list.Add(ex.Message);
                ex = ex.InnerException;
            } while (ex != null);

        }
        return string.Join(Environment.NewLine, list.Distinct().ToArray());
    }

    /// <summary>
    /// Static method distinct items by key
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="source"></param>
    /// <param name="keySelector"></param>
    /// <returns></returns>
    public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
    {
        HashSet<TKey> seenKeys = new HashSet<TKey>();
        foreach (TSource element in source)
        {
            if (seenKeys.Add(keySelector(element)))
            {
                yield return element;
            }
        }
    }

    #endregion

    /// <summary>
    /// Método de extensão para converter um valor em uma URL para ser utilizado como parametro.
    /// Basicamente converte espaços em traço e remove caracteres especiais (diátrics)
    /// </summary>
    /// <param name="source">String de origem</param>
    /// <returns>Retorna uma string para ser utilizada como parametro</returns>
    public static string AsUrl(this string source)
    {
        // Valida origem null
        if (string.IsNullOrWhiteSpace(source))
            return source;

        // Remove diátrics
        var bytes = System.Text.Encoding.GetEncoding("ISO-8859-8").GetBytes(source);
        var value = System.Text.Encoding.UTF8.GetString(bytes);

        // Separa maiúsculas de Minúsculas
        value = string.Join(" ", Regex.Split(value, @"(?<!^)(?=[A-Z])"));

        // Remove espaços substituindo por traços
        return value.Replace(" ", "-");
    }

    /// <summary>
    /// Método de extensão para renderizar um controle na tela.
    /// </summary>
    /// <param name="library"></param>
    /// <param name="values">Parâmetros</param>
    /// <returns></returns>
    public static string RenderControl(this Library library, params string[] values)
    {
        var extension = System.IO.Path.GetExtension(library.FullPath).Replace(".", "");

        if (library.LibraryType.MimeTypes.ToLower().Contains("image"))
            return $"<img class='{String.Join(" ", values)}' src='{library.FullPath}'/>";

        if (library.LibraryType.MimeTypes.ToLower().Contains("audio"))
            return $"<audio {String.Join(" ", values)}> <source src='{library.FullPath}' type='audio/{extension}'> </audio>";

        if (library.LibraryType.MimeTypes.ToLower().Contains("video"))
            return $"<video {String.Join(" ", values)}> <source src='{library.FullPath}' type ='video/{extension}> </video>";

        return $"<a class='{String.Join(" ", values)}' href='{library.FullPath}'>{library.FullPath}</a>";

    }

    /// <summary>
    /// Método que extende uma exceção e permite listar todas as exceções
    /// </summary>
    /// <param name="exception"Exceção de origem></param>
    /// <returns>Returna uma lista de dados com as informações da exceção</returns>
    public static List<string> InnerMessages(this Exception exception)
    {
        var list = new List<string>();
        var ex = exception;
        do
        {
            list.Add(ex.Message);
            ex = ex.InnerException;
        } while (ex != null);
        return list;
    }

    /// <summary>
    /// Extesion to allow system scale an image
    /// </summary>
    /// <param name="image">Image source</param>
    /// <param name="maxWidth">Max width desired</param>
    /// <param name="maxHeight">Max heigth desired</param>
    /// <returns>Returns a new instance of the image scaled</returns>
    public static Image ScaleImage(this Image image, int maxWidth, int maxHeight)
    {
        var ratioX = (double)maxWidth / image.Width;
        var ratioY = (double)maxHeight / image.Height;
        var ratio = Math.Min(ratioX, ratioY);

        var newWidth = (int)(image.Width * ratio);
        var newHeight = (int)(image.Height * ratio);

        var newImage = new Bitmap(newWidth, newHeight);

        using (var graphics = Graphics.FromImage(newImage))
            graphics.DrawImage(image, 0, 0, newWidth, newHeight);

        return newImage;
    }

    /// <summary>
    /// Extension method to compare string ignoring case
    /// </summary>
    /// <param name="source">String source to compare</param>
    /// <param name="comparer">String to comparer with</param>
    /// <returns>Returns true or false</returns>
    public static bool ContainsIgnoreCase(this string source, string comparer)
    {
        return (source?.ToLower().Contains(comparer?.ToLower() ?? "") ?? false);
    }

    /// <summary>
    /// Function to remove all the accents from a given string
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static string RemoveAccents(this string source)
    {
        if (string.IsNullOrWhiteSpace(source))
            return null;

        var encoding = Encoding.GetEncoding("iso-8859-8");
        return encoding.GetString(Encoding.Convert(Encoding.UTF8, encoding, Encoding.UTF8.GetBytes(source)));
    }

    /// <summary>
    /// Function to clear invalid chars in
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static string ClearInvalidChars(this string source)
    {
        if (string.IsNullOrWhiteSpace(source))
            return null;

        source = source.RemoveAccents();
        return source.Replace("?", "").Replace("\\", "").Replace("/", "").Replace(":", "").Replace("*", "").Replace("\"", "").Replace("<", "").Replace(">", "").Replace("|", "");
    }

    /// <summary>
    /// Helper to detect if the assembly is built in Debug mode
    /// </summary>
    /// <param name="assembly">Assembly reference to return info</param>
    /// <returns></returns>
    public static bool? IsDebug(this Assembly assembly)
        => assembly?.GetCustomAttributes(false).OfType<DebuggableAttribute>().Any(x => x.IsJITTrackingEnabled);

    #region Mail Extensions
    /// <summary>
    /// Helper method to allow view be rendered as HTML
    /// </summary>
    /// <param name="controller"></param>
    /// <param name="viewName"></param>
    /// <returns></returns>
    public static string RenderPartialViewToString(this Controller controller, string viewName)
    {
        try
        {
            if (string.IsNullOrEmpty(viewName))
                viewName = controller.RouteData.GetRequiredString("action");

            using (StringWriter sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(controller.ControllerContext, viewName);
                var viewContext = new ViewContext(controller.ControllerContext, viewResult.View, controller.ViewData, controller.TempData, sw);
                viewResult.View.Render(viewContext, sw);

                return sw.GetStringBuilder().ToString();
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    /// <summary>
    /// Method to trigger the notification mail to validate e-mail throught Token System
    /// </summary>
    /// <returns></returns>
    public static async Task TriggerNotification(this Controller controller, string type, string subject, string[] mailTo, string[] mailCc = null, string[] mailBcc = null, string[] mailReply = null, HttpFileCollectionBase attachments = null, UrlFileAttachment[] urlAttachments = null)
    {
        var entity = CMS.Functions.TemplateTypes.FirstOrDefault(f => f.Name == "Template");
        // Try to find the model in the database to be triggered
        var template = CMS.Functions.Templates.FirstOrDefault(f => f.IdTemplateType == entity.Id && f.Name.Equals($"{type}.{entity.DefaultExtension}", StringComparison.CurrentCultureIgnoreCase));
        if (template == null)
            throw new Exception(string.Format(Bitzar.CMS.Core.Resources.Strings.MailService_TemplateNotFound, type.ToString()));

        // Get custom subject for the mail template if available
        var customSubject = CMS.I18N.Text($"TemplateEmail_{type}");
        if (!string.IsNullOrWhiteSpace(customSubject))
            subject = customSubject;

        // Process view data
        var viewPath = $"{entity.DefaultPath}/{template.Name}";
        var content = controller.RenderPartialViewToString(viewPath);

        // Trigger e-mail notification
        await Mail.SendNotification(mailTo, mailCc, mailBcc, mailReply, subject, content, true, attachments, urlAttachments);
    }
    #endregion

    #region CMS Extensions
    /// <summary>
    /// Method to check if the page is a post blog
    /// </summary>
    /// <param name="page">Template source to check</param>
    /// <returns>Return true or false if the page is blog post</returns>
    public static bool IsBlogPost(this Template page)
    {
        if (page == null)
            return false;

        return CMS.Functions.TemplateTypes.FirstOrDefault(t => t.Id == page.IdTemplateType).Name == "BlogPost";
    }

    /// <summary>
    /// Extension to convert current page in blog post information
    /// </summary>
    /// <param name="page">Page source to be converted</param>
    /// <returns>Returns the instance as a blog post instance</returns>
    public static BlogPost AsBlogPost(this Template page)
    {
        /* Bind field Value if not set */
        var fields = CMS.Global.Values;
        foreach (var field in page.Fields.Where(p => p.FieldValues.Count == 0))
            field.FieldValues = fields.Where(f => f.IdField == field.Id).ToList();

        /* Start a new intance of the blog post */
        return (new BlogPost(page)).Setup();
    }

    /// <summary>
    /// Method to allocate all the data in the post
    /// </summary>
    /// <param name="post"></param>
    /// <returns></returns>
    public static BlogPost Setup(this BlogPost post)
    {
        var Page = post.Page;

        // Load content data
        post.title = CMS.Global.GetField("Título", Page, CMS.I18N.Culture.Id, 0);
        post.image = CMS.Global.GetField("Imagem", Page, CMS.I18N.Culture.Id, 0);
        post.subtitle = CMS.Global.GetField("Subtítulo", Page, CMS.I18N.Culture.Id, 0);
        post.postContent = CMS.Global.GetField("Conteúdo", Page, CMS.I18N.Culture.Id, 0);
        post.isFixed = CMS.Global.GetField("Post Fixo", Page, CMS.I18N.DefaultLanguage.Id, 0);
        post.media = CMS.Global.GetField("Mídia", Page, CMS.I18N.DefaultLanguage.Id, 0);

        var cat = CMS.Global.GetField("Categorias", Page, CMS.I18N.Culture.Id, 0);
        if (cat != null && !string.IsNullOrWhiteSpace(cat.ToString()))
            post.categories = cat.ToString().Split(',');
        else
            post.categories = new string[] { };

        var tag = CMS.Global.GetField("Tags", Page, CMS.I18N.Culture.Id, 0);
        if (tag != null && !string.IsNullOrWhiteSpace(tag.ToString()))
            post.tags = tag.ToString().Split(',');
        else
            post.tags = new string[] { };

        return post;
    }

    /// <summary>
    /// Extension method to Break list into sublists with N objects
    /// </summary>
    /// <typeparam name="T">Source type of List</typeparam>
    /// <param name="source">Source list to be breaken</param>
    /// <param name="size">Size of each sublist returned</param>
    /// <returns>Returns a collection of collection with n size.</returns>
    public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source, int size)
    {
        while (source.Any())
        {
            yield return source.Take(size);
            source = source.Skip(size);
        }
    }
    #endregion
}