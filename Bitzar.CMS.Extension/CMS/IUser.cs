using System.Collections.Generic;
using Bitzar.CMS.Data.Model;
using Bitzar.CMS.Core.Models;

namespace Bitzar.CMS.Extension.CMS
{
    public interface IUser
    {
        List<Role> AdminRoles { get; }
        List<Role> MemberRoles { get; }

        List<User> Users(bool includeDisabled = false);
        int Count(bool admin = false, string search = null, bool deepSearch = false, int idRelated = 0);
        IList<User> List(int page = 1, int size = 25, bool admin = false, string search = "", bool deepSearch = false, int idRelated = 0);
    }
}