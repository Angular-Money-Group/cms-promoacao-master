using Bitzar.CMS.Core.Helper;
using Bitzar.CMS.Core.Models;
using Bitzar.CMS.Data.Model;
using Bitzar.CMS.Extension.CMS;
using MethodCache.Attributes;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Bitzar.CMS.Core.Functions.Internal
{
    /// <summary>
    /// Class to hold and organize blog functions
    /// </summary>
    [Cache(Members.All)]
    public class Blog : Cacheable, IBlog
    {
        /// <summary>
        /// Method to get All the posts available for the current culture
        /// </summary>
        [NoCache]
        public IList<RouteParam> Posts
        {
            get => MvcApplication.AvailableRoutes.Where(r => r.Page.IsBlogPost() && r.Culture.Culture == CMS.I18N.Culture.Culture).ToList();
        }

        /// <summary>
        /// Method to search in posts and navigate in to the system
        /// </summary>
        /// <param name="page">Page number to be loaded</param>
        /// <param name="size">Size of the page to load</param>
        /// <param name="filter">Filter the posts for title and anything else</param>
        /// <param name="categories">Filter the posts for a specific category or some categories that is available</param>
        /// <param name="tags">Filter the posts for a specific tag or some tags provided</param>
        /// <returns></returns>
        [NoCache]
        public PaggedResult<BlogPost> Navigate(int page = 1, int size = 10, string filter = "", string categories = "", string tags = "")
        {
            // Get the list of posts available
            var posts = CMS.Blog.Posts.Select(f => f.Page.AsBlogPost());

            // Execute filter for title and subtitle
            if (!string.IsNullOrWhiteSpace(filter))
                posts = posts.Where(p => p.Title.ContainsIgnoreCase(filter));

            // Execute filter to match categories
            if (!string.IsNullOrWhiteSpace(categories))
                posts = posts.Where(p => p.Categories.Any(c => categories.Split(',').Select(x => x.Trim()).Any(l => c.Equals(l))));

            // Execute filter to match tags
            if (!string.IsNullOrWhiteSpace(tags))
                posts = posts.Where(p => p.Tags.Any(c => tags.Split(',').Select(x => x.Trim()).Any(l => c.Equals(l))));

            // Paginate result basead on the information
            var count = posts.Count();
            var data = posts.OrderBy(p => p.IsFixed ? 0 : 1).ThenByDescending(p => p.CreatedAt).Skip((page - 1) * size).Take(size);
            //var data = posts.OrderByDescending(p => p.CreatedAt).Skip((page - 1) * size).Take(size);

            // Prepare result to return
            return new PaggedResult<BlogPost>()
            {
                Count = count,
                Page = page,
                Size = size,
                CountPage = Convert.ToInt32(Math.Ceiling((decimal)count / size)),
                Records = data.ToList()
            };
        }

        /// <summary>
        /// Method to search in posts and navigate in to the system
        /// </summary>
        /// <param name="page">Page number to be loaded</param>
        /// <param name="size">Size of the page to load</param>
        /// <param name="filter">Filter the posts for title and anything else</param>
        /// <returns></returns>
        [NoCache]
        public PaggedResult<BlogPost> Filter(int page = 1, int size = 10, Dictionary<string, string> filter = null)
        {
            // Get the list of posts available
            var posts = CMS.Blog.Posts.Select(f => f.Page.AsBlogPost()).ToList();

            var lang = CMS.I18N.Culture;

            // Execute filters
            if (filter != null)
                foreach (var item in filter)
                    posts = (from p in posts
                             where (p[item.Key]?.FieldValues.Where(v => !string.IsNullOrWhiteSpace(v.Value))
                                                            .Any(v => v.IdLanguage == lang.Id &&
                                                                      v.Value.Contains(item.Value)) ?? false)
                             select p).ToList();

            // Paginate result basead on the information
            var count = posts.Count;
            var data = posts.OrderBy(p => p.IsFixed ? 0 : 1).ThenByDescending(p => p.CreatedAt).Skip((page - 1) * size).Take(size);

            // Prepare result to return
            return new PaggedResult<BlogPost>()
            {
                Count = count,
                Page = page,
                Size = size,
                CountPage = Convert.ToInt32(Math.Ceiling((decimal)count / size)),
                Records = data.ToList()
            };
        }

        /// <summary>
        /// Method to return the instance of the current page as a BlogPost
        /// </summary>
        [NoCache]
        public BlogPost Current
        {
            get => CMS.Page.CurrentRoute.Page.AsBlogPost();
        }

        /// <summary>
        /// Method to return all available categories in the current language
        /// </summary>
        public IList<string> Categories
        {
            get => Posts.Where(p => p.Culture.Culture == CMS.I18N.Culture.Culture).SelectMany(p => p.Page.AsBlogPost().Categories).Distinct().ToList();
        }

        /// <summary>
        /// Gt the most readed page blogs in the system.
        /// It's counted basead on the table stats
        /// </summary>
        /// <param name="size">Number of objects to return to the system</param>
        /// <param name="start">Start date to filter result</param>
        /// <param name="categories">Category list to filter data</param>
        /// <returns>Returns a list of SIZE size containing most accessed blog posts</returns>
        [NoCache]
        public IList<BlogPost> MostReaded(int size = 5, DateTime? start = null, string categories = null)
        {
            // Group pages to identify the most accessed
            var data = LoadStatistics(start);

            // Get all the page blogs available
            var posts = Posts.Where(f => f.Culture.Culture == CMS.I18N.Culture.Culture);

            // Filter categories if applies
            if (!string.IsNullOrWhiteSpace(categories))
            {
                var items = categories.Split(',');
                posts = posts.Where(p => p.Page.AsBlogPost().Categories.Any(c => items.Any(i => i.Equals(c, StringComparison.CurrentCultureIgnoreCase))));
            }

            // Math all data, and get the most accessed
            var matchData = data.Where(d => posts.Any(p => p.Route == d.Key)).OrderByDescending(d => d.Count()).Take(size);

            // return post list that is on match data
            return matchData.Select(d => posts.FirstOrDefault(p => p.Route == d.Key).Page.AsBlogPost()).ToList();
        }

        /// <summary>
        /// Internal method to cache stats by time to avoid everytime database access
        /// </summary>
        /// <param name="start">Date to start compute stats time</param>
        /// <returns>Returns an list of stats groupped</returns>
        [NoCache]
        private List<IGrouping<string, Stats>> LoadStatistics(DateTime? start = null)
        {
            try
            {
                if (HttpContext.Current.Cache["CMS.TEMP.STATS"] != null)
                    return (List<IGrouping<string, Stats>>)HttpContext.Current.Cache["CMS.TEMP.STATS"];

                using (var db = new DatabaseConnection())
                {
                    // Create stats query
                    var query = db.Stats.Where(s => !s.IsCrawler && s.Date >= (start ?? new DateTime(2000, 1, 1)));

                    // Get raw data
                    var rawData = query.ToList();

                    // Group pages to identify the most accessed
                    var data = rawData.GroupBy(s => (new Uri($"http{(s.IsSecure ? "s" : "")}://{s.Host}{s.Url}")).AbsolutePath, s => s).ToList();

                    // Store in the cache
                    HttpContext.Current.Cache.Add("CMS.TEMP.STATS", data, null, DateTime.Now.AddHours(1), System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Default, null);

                    // Return data to the system
                    return data;
                }
            }
            catch
            {
                // TO-DO: Log Exception out context
                return new List<IGrouping<string, Stats>>();
            }
        }

        /// <summary>
        /// Method to create a new blog post in the system.
        /// </summary>
        /// <param name="user">Information to record the user that has created the blog post</param>
        /// <param name="category">Specific category to start the blog post</param>
        /// <returns>Returns an instance of the current saved Post</returns>
        [NoCache]
        public Template CreateBlogPost(Data.Model.User user, string category = null)
        {
            try
            {
                using (var db = new DatabaseConnection())
                {
                    // Locate blog post template
                    var templateType = CMS.Functions.TemplateTypes.FirstOrDefault(t => t.Name == "BlogPost");

                    // Create new template of BlogPost type
                    var post = new Template()
                    {
                        Content = null,
                        Description = "Novo Post",
                        Extension = templateType.DefaultExtension,
                        IdTemplateType = templateType.Id,
                        Name = $"BlogPost-{DateTime.Now.Ticks}.{templateType.DefaultExtension}",
                        Released = false,
                        User = $"{user.FirstName} {user.LastName}",
                        Path = templateType.DefaultPath,
                        Url = $"post-{DateTime.Now.Ticks}"
                    };

                    var fieldText = CMS.Global.FieldTypes.FirstOrDefault(t => t.Name == "Texto").Id;
                    var fieldHtml = CMS.Global.FieldTypes.FirstOrDefault(t => t.Name == "Html").Id;
                    var fieldImage = CMS.Global.FieldTypes.FirstOrDefault(t => t.Name == "Imagem").Id;
                    var fieldCheck = CMS.Global.FieldTypes.FirstOrDefault(t => t.Name == "Checkbox").Id;
                    var fieldSelect = CMS.Global.FieldTypes.FirstOrDefault(t => t.Name == "Seleção").Id;

                    // Create fields to match default post information.
                    #region Configuration of Default Fields
                    post.Fields.Add(new Field() { Name = "Título", IdFieldType = fieldText, Group = "Básico", Order = 1 });
                    post.Fields.Add(new Field() { Name = "Url", IdFieldType = fieldText, Group = "Básico", Order = 2, Description = "URL amigável. Não utilize acentos, nem caracteres especiais, utilize '-' para espaço." });
                    post.Fields.Add(new Field() { Name = "Imagem", IdFieldType = fieldImage, Group = "Conteúdo", Order = 1 });
                    post.Fields.Add(new Field() { Name = "Mídia", IdFieldType = fieldText, Group = "Conteúdo", Order = 2 });
                    post.Fields.Add(new Field() { Name = "Subtítulo", IdFieldType = fieldText, Group = "Conteúdo", Order = 3 });
                    post.Fields.Add(new Field() { Name = "Conteúdo", IdFieldType = fieldHtml, Group = "Conteúdo", Order = 4 });
                    post.Fields.Add(new Field() { Name = "Categorias", IdFieldType = fieldText, Description = "Categorias separadas por , Ex: Computadores, Informática, TI, etc...", Group = "Conteúdo", Order = 5 });
                    post.Fields.Add(new Field() { Name = "Tags", IdFieldType = fieldText, Description = "Tags separadas por , Ex: tutorial, help, ajuda, know-how, kb, etc..", Group = "Conteúdo", Order = 6 });
                    post.Fields.Add(new Field() { Name = "Público", IdFieldType = fieldCheck, Group = "Outros", Order = 1, Description = "Se marcado, está disponível para acesso ao público." });
                    post.Fields.Add(new Field() { Name = "Post Fixo", IdFieldType = fieldCheck, Group = "Outros", Order = 2, Description = "Se marcado, o post será exibido sempre em primeiro." });
                    post.Fields.Add(new Field() { Name = "Publicado", IdFieldType = fieldCheck, Group = "Outros", Order = 3, Description = "Se marcado, post ficará acessível a todos os usuários." });
                    #endregion

                    // Adiciona os campos personalizados já existentes em uma postagem nova
                    var lastPost = CMS.Functions.Templates.Where(t => t.TemplateType.Name == "BlogPost").OrderByDescending(t => t.Id).FirstOrDefault();
                    var fieldList = new List<FieldValue>();
                    if (lastPost != null)
                        foreach (var field in lastPost.Fields)
                            if (!post.Fields.Any(f => f.Name == field.Name))
                            {
                                field.Id = 0;
                                field.IdTemplate = post.Id;
                                field.FieldValues = fieldList;
                                post.Fields.Add(new Field()
                                {
                                    Name = field.Name,
                                    IdFieldType = field.IdFieldType,
                                    Group = field.Group,
                                    Order = field.Order,
                                    Description = field.Description,
                                    Resource = field.Resource,
                                    SelectData = field.SelectData
                                });
                            }

                    // Create field Values for each language
                    var languages = CMS.I18N.AvailableLanguages;
                    foreach (var field in post.Fields)
                        foreach (var lang in languages)
                        {
                            var value = new FieldValue() { IdLanguage = lang.Id };
                            field.FieldValues.Add(value);

                            // Set default values
                            if (field.Name == "Título") value.Value = post.Description;
                            if (field.Name == "Url") value.Value = post.Url;
                            if (field.Name == "Publicado") value.Value = "false";
                            if (field.Name == "Post Fixo") value.Value = "false";
                            if (field.Name == "Categorias") value.Value = category;
                        }

                    // Add fields to the post and commit on the database
                    db.Templates.Add(post);
                    db.SaveChanges();

                    // Clear cache data
                    CMS.ClearCache(typeof(Functions).FullName);
                    CMS.ClearRoutes();

                    return post;
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Method to return all the available field values to the system
        /// </summary>
        /// <param name="name">Name of the current field</param>
        /// <param name="culture">Specific system culture or default culture of the request</param>
        /// <returns>Returns an object with system data</returns>
        [NoCache]
        public dynamic Field(string name, int row)
        {
            return Field(name, null, row);
        }

        /// <summary>
        /// Method to return all the available field values to the system
        /// </summary>
        /// <param name="name">Name of the current field</param>
        /// <param name="culture">Specific system culture or default culture of the request</param>
        /// <param name="row">Indicates what row should be loaded from database</param>
        /// <returns>Returns an object with system data</returns>
        [NoCache]
        public dynamic Field(string name, int? culture = null, int row = 0)
        {
            // Method to return a field to the system
            var page = CMS.Page.CurrentRoute.Page;
            if (page == null)
                return null;

            var lang = culture ?? CMS.I18N.Culture.Id;

            // Locate the field availability for the template
            return CMS.Global.GetField(name, page, lang, row);
        }

        /// <summary>
        /// Method to return all the related data from a repeated and their children objects
        /// </summary>
        /// <param name="name">Name of the current repeater</param>
        /// <param name="culture">Specific system culture or default culture of the request</param>
        /// <returns>Returns an object with system data</returns>
        [NoCache]
        public List<IGrouping<int, KeyValuePair<Field, dynamic>>> Repeater(string name, int? culture = null)
        {
            // Method to return a field to the system
            var page = CMS.Page.CurrentRoute.Page;
            if (page == null)
                return null;

            var lang = culture ?? CMS.I18N.Culture.Id;

            return CMS.Global.GetRepeater(name, page, lang);
        }
    }
}