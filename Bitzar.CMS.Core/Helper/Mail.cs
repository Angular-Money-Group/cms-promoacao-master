using Bitzar.CMS.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Bitzar.CMS.Core.Helper
{
    /// <summary>
    /// Class to set the current system mail configuration
    /// </summary>
    public class Mail
    {
        /// <summary>
        /// Método utilizado para envio de notificações por e-mail
        /// </summary>
        /// <param name="mailTo">E-mail que recebera a notificação</param>
        /// <param name="mailCc">Lista de e-mails para cópia das notificações</param>
        /// <param name="mailBcc">Lista de e-mails para cópia oculta das notificações</param>
        /// <param name="mailReply">Lista de e-mails para resposta</param>
        /// <param name="subject">Assunto da Mensagem</param>
        /// <param name="body">Corpo do e-mail</param>
        /// <param name="isBodyHtml">Se formato aceito é html ou não</param>
        /// <param name="attachments">Lista de anexos para enviar junto com o e-mail</param>
        public static async Task SendNotification(string[] mailTo, string[] mailCc, string[] mailBcc, string[] mailReply, string subject, string body, bool isBodyHtml, HttpFileCollectionBase attachments = null, UrlFileAttachment[] urlAttachments = null)
        {
            try
            {
                // Load parameter variables
                var smtpHost = Functions.CMS.Configuration.Get("SmtpHost");
                if (string.IsNullOrWhiteSpace(smtpHost))
                    throw new Exception(Resources.Strings.Configuration_MailServiceNotDefined);

                var smtpPort = Convert.ToInt32(Functions.CMS.Configuration.Get("SmtpPort"));
                var smtpSender = Functions.CMS.Configuration.Get("SmtpSender");
                var senderName = Functions.CMS.Configuration.Get("SmtpDisplayName");
                var smtpUser = Functions.CMS.Configuration.Get("SmtpUser");
                var smtpPswd = Functions.CMS.Configuration.Get("SmtpPassword");
                var smtpSsl = (Functions.CMS.Configuration.Get("SmtpSSL") == "true");

                // Create mail message
                using (var mail = new MailMessage())
                {
                    foreach (var item in mailTo.Distinct())
                        mail.To.Add(item);

                    /* Add CC/BCC/Reply Lists */
                    if (mailCc != null && mailCc.Length > 0)
                        foreach (var item in mailCc.Distinct())
                            mail.CC.Add(item);
                    if (mailBcc != null && mailBcc.Length> 0)
                        foreach (var item in mailBcc.Distinct())
                        mail.Bcc.Add(item);
                    if (mailReply != null && mailReply.Length > 0)
                        foreach (var item in mailReply.Distinct())
                        mail.ReplyToList.Add(item);

                    mail.From = new MailAddress(smtpSender, senderName);
                    mail.Subject = subject;
                    mail.SubjectEncoding = Encoding.UTF8;
                    mail.Body = body;
                    mail.BodyEncoding = Encoding.UTF8;
                    mail.IsBodyHtml = isBodyHtml;

                    // Add attachments if exists
                    if (attachments != null && attachments.Count > 0)
                        foreach (var key in attachments.AllKeys)
                        {
                            var contentType = new System.Net.Mime.ContentType()
                            {
                                MediaType = System.Net.Mime.MediaTypeNames.Application.Octet,
                                Name = attachments[key].FileName
                            };
                            mail.Attachments.Add(new Attachment(attachments[key].InputStream, contentType));
                        }

                    // Add url attachments if exists
                    if (urlAttachments != null && urlAttachments.Length > 0)
                        foreach (var attachment in urlAttachments)
                        {
                            var contentType = new System.Net.Mime.ContentType()
                            {
                                MediaType = System.Net.Mime.MediaTypeNames.Application.Octet,
                                Name = attachment.FileName
                            };

                            mail.Attachments.Add(new Attachment(attachment.Stream, contentType));
                        }

                    // Create smtp client
                    var smtp = new SmtpClient(smtpHost, smtpPort)
                    {
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential(smtpUser, smtpPswd),
                        EnableSsl = smtpSsl,
                        Timeout = 60
                    };
                    await smtp.SendMailAsync(mail);

                    // Dispose attachments
                    foreach (var attachment in mail.Attachments)
                        attachment.Dispose();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message + Environment.NewLine + ex.StackTrace;
                Functions.CMS.Log.LogRequest(message);
                throw ex;
            }
        }
    }
}