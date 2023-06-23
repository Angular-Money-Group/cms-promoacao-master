using Bitzar.CMS.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bitzar.CMS.Core.Models
{
    public class MembershipAuthentication
    {
        public User User { get; set; }
        public DateTimeOffset Start { get; set; } = DateTimeOffset.Now;
        public DateTimeOffset LastRequest { get; set; } = DateTimeOffset.Now;

        public MembershipAuthentication(User user)
        {
            this.User = user;
        }
    }
}