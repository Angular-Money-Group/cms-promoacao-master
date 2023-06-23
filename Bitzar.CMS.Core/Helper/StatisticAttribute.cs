using Bitzar.CMS.Core.Models;
using Bitzar.CMS.Data.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using System.Web.Routing;

namespace Bitzar.CMS.Core.Helper
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public class StatisticAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Método acionado toda vez que uma ação é iniciada no sistema
        /// </summary>
        /// <param name="filterContext">contexto onde a ação é requisitada</param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var controller = filterContext.Controller;
            if (controller != null)
            {
                var timer = Stopwatch.StartNew();
                controller.ViewData["CMS.BITZAR.TIMER"] = timer;
            }

            base.OnActionExecuting(filterContext);
        }

        /// <summary>
        /// Método acionado quando uma ação no sistema é finalizada
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var elapsedTime = (long)0;
            try
            {
                var controller = filterContext.Controller;
                if (controller == null)
                    return;

                var timer = (Stopwatch)controller.ViewData["CMS.BITZAR.TIMER"];
                if (timer == null)
                    return;

                timer.Stop();
                elapsedTime = timer.ElapsedMilliseconds;
            }
            finally
            {
                RecordSystemStatistic(filterContext, elapsedTime);
                base.OnActionExecuted(filterContext);
            }
        }

        /// <summary>
        /// Método de armazenamento assíncrono das estatísticas de uso do sistema
        /// </summary>
        /// <param name="elapsedTime"></param>
        private void RecordSystemStatistic(ActionExecutedContext filterContext, long elapsedTime)
        {
            try
            {
                // Store global vars
                var name = Functions.CMS.Membership.User?.UserName ?? null;
                var ip = filterContext.RequestContext.HttpContext.Request.UserHostAddress;
                var url = filterContext.RequestContext.HttpContext.Request.Url;
                var referrer = filterContext.RequestContext.HttpContext.Request.UrlReferrer;
                var response = filterContext.RequestContext.HttpContext.Response.StatusCode;
                var session = filterContext.RequestContext.HttpContext.Session.SessionID;
                var type = filterContext.RequestContext.HttpContext.Request.HttpMethod;
                var clearHistory = filterContext.HttpContext.Cache.Get("CMS.BITZAR.CLEARHISTORY") == null;
                var browser = filterContext.RequestContext.HttpContext.Request.Browser;
                var localhost = ip == "::1";

                // Set clear history to not execute again
                if (clearHistory)
                    filterContext.HttpContext.Cache.Add("CMS.BITZAR.CLEARHISTORY", true, null, DateTime.Now.AddHours(24), Cache.NoSlidingExpiration, CacheItemPriority.Default, null);

                // Store in the database asyn
                ThreadPool.QueueUserWorkItem(delegate
                {
                    try
                    {
                        using (var db = new DatabaseConnection())
                        {
                            // Clear old Records
                            if (clearHistory)
                            {
                                var days = Convert.ToInt32(Functions.CMS.Configuration.Get("KeepStatisticsPeriod") ?? "30");
                                var dateParam = DateTime.Now.AddDays(-1 * days);
                                var history = db.Stats.Where(d => d.Date < dateParam).ToList();
                                db.Stats.RemoveRange(history);
                            }

                            // Record Stats
                            db.Stats.Add(new Stats()
                            {
                                Host = url.Host,
                                HttpResult = response.ToString(),
                                Ip = ip,
                                IsSecure = (url.Scheme.Equals("https", StringComparison.CurrentCultureIgnoreCase)),
                                Session = session,
                                Time = elapsedTime,
                                Type = type,
                                Url = url.PathAndQuery,
                                UrlReferrer = referrer?.ToString(),
                                UserName = name,
                                Browser = browser?.Browser,
                                Version = browser?.MajorVersion.ToString(),
                                IsCrawler = localhost || (browser?.Crawler ?? false),
                                IsMobileDevice = browser?.IsMobileDevice ?? false,
                                MobileManufacturer = browser?.MobileDeviceManufacturer,
                                MobileModel = browser?.MobileDeviceModel
                            });

                            db.SaveChanges();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.AllMessages());
                        Trace.WriteLine(ex.AllMessages());
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.AllMessages());
                Trace.WriteLine(ex.AllMessages());
            }
        }
    }
}