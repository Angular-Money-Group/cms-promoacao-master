using Bitzar.CMS.Core.Helper;
using Bitzar.CMS.Data.Model;
using Bitzar.CMS.Extension.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Bitzar.CMS.Core.Areas.admin.Controllers
{
    [RouteArea("Admin", AreaPrefix = "")]
    public class DefaultController : AdminBaseController
    {
        // GET: admin/Default
        [Route("Admin")]
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Return partial view for online current users stats
        /// </summary>
        /// <returns></returns>
        [Route("Admin/Estatisticas/Usuario-OnLine")]
        public async Task<ActionResult> StatisticOnlineVisitors()
        {
            try
            {
                using (var db = new DatabaseConnection())
                {
                    var dateRef = DateTime.Now.AddMinutes(-1);
                    var stats = await db.Stats.Where(s => !s.IsCrawler && s.Date > dateRef).ToListAsync();

                    return PartialView("_StatsOnLine", stats);
                }
            }
            catch (Exception ex)
            {
                var parameters = new
                {
                    Exception = ex,
                    Controller = this.Request.RequestContext.RouteData.DataTokens["controller"]?.ToString(),
                    Action = this.Request.RequestContext.RouteData.DataTokens["action"]?.ToString(),              
                    Url = this.Request.Url.ToString()
                };

                Functions.CMS.Log.LogRequest(parameters);
                return PartialView("_StatsOnLine", null);
            }
        }

        /// <summary>
        /// Return partial view for online current users stats
        /// </summary>
        /// <returns></returns>
        [Route("Admin/Estatisticas/Usuarios-Hoje")]
        public async Task<ActionResult> StatisticTodayVisitors()
        {
            try
            {
                using (var db = new DatabaseConnection())
                {
                    var dateRef = DateTime.Now.Date;
                    var stats = await db.Stats.Where(s => !s.IsCrawler && s.Date > dateRef).ToListAsync();

                    return PartialView("_StatsTodayVisitors", stats);
                }
            }
            catch (Exception ex)
            {
                var parameters = new
                {
                    Exception = ex,
                    Controller = this.Request.RequestContext.RouteData.DataTokens["controller"]?.ToString(),
                    Action = this.Request.RequestContext.RouteData.DataTokens["action"]?.ToString(),
                    Url = this.Request.Url.ToString()
                };
                Functions.CMS.Log.LogRequest(parameters);

                return PartialView("_StatsTodayVisitors", null);
            }
        }

        /// <summary>
        /// Return partial view for online current users stats
        /// </summary>
        /// <returns></returns>
        [Route("Admin/Estatisticas/Total-Acessos")]
        public async Task<ActionResult> StatisticTodayRequests()
        {
            try
            {
                using (var db = new DatabaseConnection())
                {
                    var dateRef = DateTime.Now.Date;
                    var stats = await db.Stats.Where(s => !s.IsCrawler && s.Date > dateRef).ToListAsync();

                    return PartialView("_StatsTodayRequests", stats);
                }
            }
            catch (Exception ex)
            {
                var parameters = new
                {
                    Exception = ex,
                    Controller = this.Request.RequestContext.RouteData.DataTokens["controller"]?.ToString(),
                    Action = this.Request.RequestContext.RouteData.DataTokens["action"]?.ToString(),
                    Url = this.Request.Url.ToString()
                };
                Functions.CMS.Log.LogRequest(parameters);
                return PartialView("_StatsTodayRequests", null);
            }
        }

        /// <summary>
        /// Return partial view for online current users stats
        /// </summary>
        /// <returns></returns>
        [Route("Admin/Estatisticas/Usuario-Ultimos-7-dias")]
        public async Task<ActionResult> StatisticLast7Days()
        {
            try
            {
                using (var db = new DatabaseConnection())
                {
                    var dateRef = DateTime.Now.Date.AddDays(-7); ;
                    var stats = await db.Stats.Where(s => !s.IsCrawler && s.Date > dateRef).ToListAsync();

                    return PartialView("_StatsLast7Days", stats);
                }
            }
            catch (Exception ex)
            {
                var parameters = new
                {
                    Exception = ex,
                    Controller = this.Request.RequestContext.RouteData.DataTokens["controller"]?.ToString(),
                    Action = this.Request.RequestContext.RouteData.DataTokens["action"]?.ToString(),
                    Url = this.Request.Url.ToString()
                };
                Functions.CMS.Log.LogRequest(parameters);
                return PartialView("_StatsLast7Days", null);
            }
        }

        /// <summary>
        /// Return partial view for online current users stats
        /// </summary>
        /// <returns></returns>
        [Route("Admin/Estatisticas/Paginas-Hoje")]
        public async Task<ActionResult> StatisticTopPagesToday()
        {
            try
            {
                using (var db = new DatabaseConnection())
                {
                    var dateRef = DateTime.Now.Date;
                    var stats = await db.Stats.Where(s => !s.IsCrawler && s.Date > dateRef).ToListAsync();

                    return PartialView("_StatsPagesToday", stats);
                }
            }
            catch (Exception ex)
            {
                var parameters = new
                {
                    Exception = ex,
                    Controller = this.Request.RequestContext.RouteData.DataTokens["controller"]?.ToString(),
                    Action = this.Request.RequestContext.RouteData.DataTokens["action"]?.ToString(),
                    Url = this.Request.Url.ToString()
                };
                Functions.CMS.Log.LogRequest(parameters);
                return PartialView("_StatsPagesToday", null);
            }
        }

        /// <summary>
        /// Return partial view for online current users stats
        /// </summary>
        /// <returns></returns>
        [Route("Admin/Estatisticas/Paginas-Ultimos-7-dias")]
        public async Task<ActionResult> StatisticTopPages7Days()
        {
            try
            {
                using (var db = new DatabaseConnection())
                {
                    var dateRef = DateTime.Now.Date.AddDays(-7);
                    var stats = await db.Stats.Where(s => !s.IsCrawler && s.Date > dateRef).ToListAsync();

                    return PartialView("_StatsPages7Day", stats);
                }
            }
            catch (Exception ex)
            {
                var parameters = new
                {
                    Exception = ex,
                    Controller = this.Request.RequestContext.RouteData.DataTokens["controller"]?.ToString(),
                    Action = this.Request.RequestContext.RouteData.DataTokens["action"]?.ToString(),
                    Url = this.Request.Url.ToString()
                };
                Functions.CMS.Log.LogRequest(parameters);
                return PartialView("_StatsPages7Day", null);
            }
        }

        /// <summary>
        /// Return partial view for online current users stats
        /// </summary>
        /// <returns></returns>
        [Route("Admin/Estatisticas/Painel-Ultimos-30-dias")]
        public async Task<ActionResult> StatisticPanel30Days()
        {
            try
            {
                using (var db = new DatabaseConnection())
                {
                    var dateRef = DateTime.Now.Date.AddDays(-7);
                    var stats = await db.Stats.Where(s => !s.IsCrawler && s.Date > dateRef).ToListAsync();

                    return PartialView("_StatsPanel30Day", stats);
                }
            }
            catch (Exception ex)
            {
                var parameters = new
                {
                    Exception = ex,
                    Controller = this.Request.RequestContext.RouteData.DataTokens["controller"]?.ToString(),
                    Action = this.Request.RequestContext.RouteData.DataTokens["action"]?.ToString(),
                    Url = this.Request.Url.ToString()
                };
                Functions.CMS.Log.LogRequest(parameters);
                return PartialView("_StatsPanel30Day", null);
            }
        }

        /// <summary>
        /// Return partial view for the number of site members
        /// </summary>
        /// <returns></returns>
        [Route("Admin/Estatisticas/Total-Usuarios")]
        public ActionResult TotalUserCount()
        {
            try
            {
                using (var db = new DatabaseConnection())
                {
                    var members = Functions.CMS.User.Users().Where(u => !u.AdminAccess).Count();
                    return PartialView("_TotalUserCount", members);
                }
            }
            catch (Exception ex)
            {
                var parameters = new
                {
                    Exception = ex,
                    Controller = this.Request.RequestContext.RouteData.DataTokens["controller"]?.ToString(),
                    Action = this.Request.RequestContext.RouteData.DataTokens["action"]?.ToString(),
                    Url = this.Request.Url.ToString()
                };
                Functions.CMS.Log.LogRequest(parameters);
                return PartialView("_TotalUserCount", null);
            }
        }

        /// <summary>
        /// Return partial view for the number of site members that had activated
        /// </summary>
        /// <returns></returns>
        [Route("Admin/Estatisticas/Total-Usuarios-Ativados")]
        public ActionResult TotalActivatedUser()
        {
            try
            {
                using (var db = new DatabaseConnection())
                {
                    var members = Functions.CMS.Membership.CountActivated();
                    return PartialView("_TotalActiveUser", members);
                }
            }
            catch (Exception ex)
            {
                var parameters = new
                {
                    Exception = ex,
                    Controller = this.Request.RequestContext.RouteData.DataTokens["controller"]?.ToString(),
                    Action = this.Request.RequestContext.RouteData.DataTokens["action"]?.ToString(),
                    Url = this.Request.Url.ToString()
                };
                Functions.CMS.Log.LogRequest(parameters);
                return PartialView("_TotalActiveUser", null);
            }
        }

        /// <summary>
        /// Return partial view for the number of site members had accessed the site in last 7 days
        /// </summary>
        /// <returns></returns>
        [Route("Admin/Estatisticas/Total-Usuarios-7Dias")]
        public ActionResult TotalUser7Days()
        {
            try
            {
                using (var db = new DatabaseConnection())
                {
                    var dateRef = DateTime.Now.Date.AddDays(-7);
                    var members = Functions.CMS.Membership.Members(1, int.MaxValue).Records.Where(m => m.LastLogin >= dateRef).ToList();
                    return PartialView("_TotalUser7Days", members);
                }
            }
            catch (Exception ex)
            {
                var parameters = new
                {
                    Exception = ex,
                    Controller = this.Request.RequestContext.RouteData.DataTokens["controller"]?.ToString(),
                    Action = this.Request.RequestContext.RouteData.DataTokens["action"]?.ToString(),
                    Url = this.Request.Url.ToString()
                };
                Functions.CMS.Log.LogRequest(parameters);
                return PartialView("_TotalUser7Days", null);
            }
        }

        /// <summary>
        /// Return partial view for the number of site members had accessed the site today
        /// </summary>
        /// <returns></returns>
        [Route("Admin/Estatisticas/Total-Usuarios-Hoje")]
        public ActionResult TotalUserToday()
        {
            try
            {
                using (var db = new DatabaseConnection())
                {
                    var dateRef = DateTime.Now.Date;
                    var members = Functions.CMS.Membership.Members(1, int.MaxValue).Records.Where(m => m.LastLogin >= dateRef).ToList();
                    return PartialView("_TotalUserToday", members);
                }
            }
            catch (Exception ex)
            {
                var parameters = new
                {
                    Exception = ex,
                    Controller = this.Request.RequestContext.RouteData.DataTokens["controller"]?.ToString(),
                    Action = this.Request.RequestContext.RouteData.DataTokens["action"]?.ToString(),
                    Url = this.Request.Url.ToString()
                };
                Functions.CMS.Log.LogRequest(parameters);
                return PartialView("_TotalUserToday", null);
            }
        }

        /// <summary>
        /// Plugin metrics information
        /// </summary>
        /// <returns></returns>
        [Route("Admin/Estatisticas/Metricas-Plugins")]
        public ActionResult PluginMetrics()
        {
            try
            {
                var list = new List<IMetric>();
                foreach (var plugin in Functions.CMS.Plugins.Available)
                    list.AddRange(plugin.Plugin.Metrics());

                return PartialView("_PluginMetric", list);
            }
            catch (Exception ex)
            {
                var parameters = new
                {
                    Exception = ex,
                    Controller = this.Request.RequestContext.RouteData.DataTokens["controller"]?.ToString(),
                    Action = this.Request.RequestContext.RouteData.DataTokens["action"]?.ToString(),
                    Url = this.Request.Url.ToString()
                };
                Functions.CMS.Log.LogRequest(parameters);
                return PartialView("_PluginMetric", null);
            }
        }

        /// <summary>
        /// Plugin metrics information for an specific page
        /// </summary>
        /// <returns></returns>
        [Route("Admin/Estatisticas/Metricas-Plugins-Pagina")]
        public ActionResult PluginMetricsPage()
        {
            try
            {
                var page = Request.Url.AbsolutePath;

                var list = new List<IMetric>();
                foreach (var plugin in Functions.CMS.Plugins.Available)
                {
                    var metrics = plugin.Plugin.Metrics().Where(m => m.Page == page);
                    if (metrics.Any())
                        list.AddRange(metrics);
                }

                return PartialView("_PluginMetric", list);
            }
            catch (Exception ex)
            {
                var parameters = new
                {
                    Exception = ex,
                    Controller = this.Request.RequestContext.RouteData.DataTokens["controller"]?.ToString(),
                    Action = this.Request.RequestContext.RouteData.DataTokens["action"]?.ToString(),
                    Url = this.Request.Url.ToString()
                };
                Functions.CMS.Log.LogRequest(parameters);
                return PartialView("_PluginMetric", null);
            }
        }
    }
}