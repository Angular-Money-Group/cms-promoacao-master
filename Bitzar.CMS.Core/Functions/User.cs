using Bitzar.CMS.Core.Helper;
using Bitzar.CMS.Core.Models;
using Bitzar.CMS.Data.Model;
using Bitzar.CMS.Extension.CMS;
using MethodCache.Attributes;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Bitzar.CMS.Core.Functions.Internal
{
    /// <summary>
    /// Class to hold and organize library functions
    /// </summary>
    [Cache(Members.All)]
    public class User : Cacheable, IUser
    {

        /// <summary>
        /// Method to Load all the roles available
        /// </summary>
        /// <returns></returns>
        private List<Role> Roles
        {
            get
            {
                using (var db = new DatabaseConnection())
                    return db.Roles.ToList();
            }
        }

        /// <summary>
        /// Method to Load all the member roles available
        /// </summary>
        /// <returns></returns>
        public List<Role> MemberRoles
        {
            get => Roles.Where(r => !r.AdminRole).ToList();
        }

        /// <summary>
        /// Method to Load all the member roles available
        /// </summary>
        /// <returns></returns>
        public List<Role> AdminRoles
        {
            get => Roles.Where(r => r.AdminRole).ToList();
        }

        /// <summary>
        /// Method to Load all the users that are not disabled
        /// </summary>
        /// <returns></returns>
        public List<Data.Model.User> Users(bool includeDisabled = false)
        {
            using (var db = new DatabaseConnection())
            {
                var source = db.Users.Include(u => u.Role).Include(u => u.UserFields).Where(u => !u.Deleted);

                // if include disabled return all users that exists on the database
                if (includeDisabled)
                    return source.ToList();

                // Or only return available users to show
                return (from u in source
                        where !u.Disabled
                        select u).ToList();
            }
        }

        /// <summary>
        /// Method to count the number of users that exists in the system
        /// </summary>
        /// <param name="admin">Indicates if the users are admin or not</param>
        /// <param name="search">Search parameter to lookup user information</param>
        /// <param name="deepSearch">Indicate that search should be done in user fields</param>
        /// <param name="idRelated">indicates which users should appear</param>
        /// <returns></returns>
        [NoCache]
        public int Count(bool admin = false, string search = "", bool deepSearch = false, int idRelated = 0)
        {
            // Get user Source
            var source = idRelated != 0 ? Users(true).Where(u => u.AdminAccess == admin && u.IdParent == idRelated) : Users(true).Where(u => u.AdminAccess == admin);

            var data = new List<Data.Model.User>();

            // Filter data if has filter specified
            if (string.IsNullOrWhiteSpace(search))
                data = source.ToList();
            else
            {
                data.AddRange(source.Where(u => u.FirstName.ContainsIgnoreCase(search) || u.LastName.ContainsIgnoreCase(search) || u.Email.ContainsIgnoreCase(search) || u.UserName.ContainsIgnoreCase(search)));

                // Set the query to lookup in deep fields
                if (deepSearch)
                    data.AddRange(source.Where(u => u.UserFields.Any(f => f.Value.ContainsIgnoreCase(search))));
            }

            // Return data to show
            return data.DistinctBy(u => u.Id).Count();
        }

        /// <summary>
        /// Method to get and search members in the Member List
        /// </summary>
        /// <param name="admin">Indicates if the users are admin or not</param>
        /// <param name="page">Current page of data</param>
        /// <param name="size">Size of each page to partition result</param>
        /// <param name="search">Search term if desired</param>
        /// <param name="deepSearch">Indicate that search should be done in user fields</param>
        /// <param name="idRelated">indicates which users should appear</param>
        /// <returns>Returns a list of members that match parameters criteria</returns>
        [NoCache]
        public IList<Data.Model.User> List(int page = 1, int size = 25, bool admin = false, string search = "", bool deepSearch = false, int idRelated = 0)
        {
            // Get user Source
            var source = Users(true).Where(u => u.AdminAccess == admin);
            if (idRelated != 0 && !admin)
            {
                source = source.Where(f => f.IdParent == idRelated);
            }
            var data = new List<Data.Model.User>();


            // Filter user for non-admin access
            if (admin && CMS.Membership.IsAdminAuthenticated && CMS.Membership.AdminUser.Role.Name != "Administrador")
                return source.Where(u => u.Id == CMS.Membership.AdminUser.Id).ToList();

            // Filter data if has filter specified
            if (string.IsNullOrWhiteSpace(search))
                data = source.ToList();
            else
            {
                data.AddRange(source.Where(u => u.FirstName.ContainsIgnoreCase(search) || u.LastName.ContainsIgnoreCase(search) || u.Email.ContainsIgnoreCase(search) || u.UserName.ContainsIgnoreCase(search)));

                // Set the query to lookup in deep fields
                if (deepSearch)
                    data.AddRange(source.Where(u => u.UserFields.Any(f => f.Value.ContainsIgnoreCase(search))));
            }

            // Return data to show
            return data.DistinctBy(u => u.Id).OrderBy(s => s.Id).Skip((page - 1) * size).Take(size).ToList();
        }

        /// <summary>
        /// Método de replicar campos para usuários
        /// </summary>
        public void ReplicateFieldsUsers()
        {
            using (var db = new DatabaseConnection())
            {
                var sql = new StringBuilder();
                sql.AppendLine("INSERT INTO btz_userfield (IdUser, Name, Value)");
                sql.AppendLine("SELECT  DISTINCT");
                sql.AppendLine("        U.id AS idUser, F.Name, '' AS Value");
                sql.AppendLine("FROM   (SELECT  DISTINCT Name");
                sql.AppendLine("        FROM    btz_userfield");
                sql.AppendLine("        WHERE   Name != '') F");
                sql.AppendLine("        CROSS JOIN btz_user U");
                sql.AppendLine("        LEFT  JOIN btz_userfield X ON U.id = X.idUser AND F.Name = X.Name");
                sql.AppendLine("WHERE   X.Id IS NULL");

                try
                {
                    db.Database.ExecuteSqlCommand(sql.ToString());
                }
                catch (Exception e)
                {
                    throw e;
                }
            }   
        }

    }
}