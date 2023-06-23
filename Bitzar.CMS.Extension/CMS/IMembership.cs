using Bitzar.CMS.Core.Models;
using Bitzar.CMS.Data.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bitzar.CMS.Extension.CMS
{
    public interface IMembership
    {
        MembershipAuthentication Authorization { get; }
        bool IsAuthenticated { get; }
        User User { get; }
        bool IsAdminAuthenticated { get; }
        User AdminUser { get; }
        PaggedResult<User> Members(int page = 1, int size = 25, string search = "", bool deepSearch = false);
        IList<User> Members();
        int Count();
        int CountActivated();
        void SetAuthenticationMember(User member);
    }
}