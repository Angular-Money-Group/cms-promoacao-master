using Bitzar.CMS.Core.Helper;
using Bitzar.CMS.Core.Models;
using Bitzar.CMS.Data.Model;
using Bitzar.CMS.Extension.CMS;
using MethodCache.Attributes;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Bitzar.CMS.Core.Functions.Internal
{
    /// <summary>
    /// Class to hold and organize library functions
    /// </summary>
    [Cache(MethodCache.Attributes.Members.All)]
    public class Membership : Cacheable, IMembership
    {
        /// <summary>
        /// Property to indicate if the current user is authenticated in the platform
        /// </summary>
        [NoCache]
        public bool IsAuthenticated => HttpContext.Current?.Session?[Extensions.NameOfType<MembershipAuthentication>()] != null;

        /// <summary>
        /// Property to get the current authenticated user
        /// </summary>
        [NoCache]
        public Data.Model.User User => Authorization?.User;

        /// <summary>
        /// Property to get the current Authorization data
        /// </summary>
        [NoCache]
        public MembershipAuthentication Authorization => (MembershipAuthentication)HttpContext.Current?.Session[Extensions.NameOfType<MembershipAuthentication>()];

        /// <summary>
        /// Property to indicate if an Admin User is authenticated
        /// </summary>
        [NoCache]
        public bool IsAdminAuthenticated => HttpContext.Current?.Session[Extensions.NameOfType<User>()] != null;

        /// <summary>
        /// Property to get the current authenticated admin user.
        /// </summary>
        [NoCache]
        public Data.Model.User AdminUser => (Data.Model.User)HttpContext.Current?.Session[Extensions.NameOfType<User>()];

        /// <summary>
        /// Hidden property out of IMembership to use for select user information
        /// </summary>
        [NoCache]
        public Data.Model.User SelectedUser { get; internal set; }

        /// <summary>
        /// Method to get and search members in the Member List
        /// </summary>
        /// <param name="page">Current page of data</param>
        /// <param name="size">Size of each page to partition result</param>
        /// <param name="search">Search term if desired</param>
        /// <returns>Returns a list of members that match parameters criteria</returns>
        [NoCache]
        public PaggedResult<Data.Model.User> Members(int page = 1, int size = 25, string search = "", bool deepSearch = false)
        {
            using (var db = new DatabaseConnection())
            {
                var source = db.Users.Include(u => u.Role).Include(u => u.UserFields).Where(u => !u.Disabled);

                // Filter data if has filter specified
                if (!string.IsNullOrWhiteSpace(search))
                    source = source.Where(u => u.FirstName.Contains(search) || u.LastName.Contains(search) || u.Email.Contains(search) || u.UserName.Contains(search));

                // Set the query to lookup in deep fields
                if (deepSearch)
                    source = source.Where(u => u.UserFields.Any(f => f.Value.Contains(search)));

                // Return data to show
                var count = source.Count();
                var data = source.OrderBy(s => s.Id).Skip((page - 1) * size).Take(size).ToList();

                // Prepare result to return
                return new PaggedResult<Data.Model.User>()
                {
                    Count = count,
                    Page = page,
                    Size = size,
                    CountPage = Convert.ToInt32(Math.Ceiling((decimal)count / size)),
                    Records = data
                };
            }
        }

        /// <summary>
        /// Method to get all the members available in the database
        /// </summary>
        /// <returns>Returns a list with all the available members</returns>
        public IList<Data.Model.User> Members()
        {
            using (var db = new DatabaseConnection())
                return db.Users.Include(u => u.Role).Include(u => u.UserFields).Where(u => !u.Disabled).ToList();
        }

        /// <summary>
        /// Method to get the number of members in the system
        /// </summary>
        /// <returns>Returns the amount of members available in the system</returns>
        public int Count()
        {
            using (var db = new DatabaseConnection())
                return db.Database.SqlQuery<int>("SELECT COUNT(*) FROM btz_user WHERE Disabled = 0 AND AdminAccess = 0").FirstOrDefault();
        }

        /// <summary>
        /// Method to get the number of activated members in the system
        /// </summary>
        /// <returns>Returns the amount of members that are activated</returns>
        public int CountActivated()
        {
            using (var db = new DatabaseConnection())
                return db.Database.SqlQuery<int>("SELECT COUNT(*) FROM btz_user WHERE Disabled = 0 AND AdminAccess = 0 AND Validated IS NOT NULL").FirstOrDefault();
        }

        /// <summary>
        /// Method to list all permissions
        /// </summary>
        public List<Data.Model.RolePermission> Permissions
        {
            get
            {
                using (var db = new DatabaseConnection())
                    return db.RolesPermissions.ToList();
            }
        }

        /// <summary>
        /// Method to check if permission is valid
        /// </summary>
        /// <param name="idRole"></param>
        /// <param name="source"></param>
        /// <param name="module"></param>
        /// <param name="function"></param>
        /// <returns></returns>
        [NoCache]
        public bool MemberHasAccess(int idRole, string source, string module = null, string function = null)
        {
            if (idRole == 1)
                return true;

            var permissions = Permissions.Where(p => p.IdRole == idRole && p.Source == source);

            // Check if role is not allowed in source
            if (permissions.FirstOrDefault(p => p.Module == null)?.Status == RolePermission.PermissionType.Deny)
                return false;

            // Check if role is not allowed in module
            if (permissions.FirstOrDefault(p => p.Module == module && p.Function == null)?.Status == RolePermission.PermissionType.Deny)
                return false;

            // Check if role is not allowed in function
            if (permissions.FirstOrDefault(p => p.Module == module && p.Function == function)?.Status == RolePermission.PermissionType.Deny)
                return false;

            return true;
        }

        /// <summary>
        /// Method to check if permission is valid
        /// </summary>
        /// <param name="source"></param>
        /// <param name="module"></param>
        /// <param name="function"></param>
        /// <returns></returns>
        [NoCache]
        public bool MemberHasAccess(string source, string module = null, string function = null) => MemberHasAccess(AdminUser?.IdRole ?? 0, source, module, function);
        
        /// <summary>
        /// Private method to set member in the logged in state
        /// </summary>
        /// <param name="member">Member instance that are logged in the system</param>
        /// <returns></returns>
        public void SetAuthenticationMember(Data.Model.User member)
        {
            try
            {
                using (var db = new DatabaseConnection())
                {
                    var authorization = new MembershipAuthentication(member);
                    HttpContext.Current.Session.Set(authorization);
                    
                    // Update member last access
                    member.LastLogin = DateTime.Now;
                    db.SaveChanges();
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }            
        }
    }
}