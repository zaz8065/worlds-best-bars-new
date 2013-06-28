using System;
using System.Linq;
using System.Security.Principal;
using WorldsBestBars.Services;

namespace WorldsBestBars.Web.Admin.Security
{
    public class MemberPrincipal : IPrincipal
    {
        public IIdentity Identity { get; private set; }

        public bool IsInRole(string role)
        {
            if (Identity is MemberIdentity)
            {
                return (Identity as MemberIdentity).Roles.Contains(role);
            }

            return false;
        }

        public static MemberPrincipal Get(Guid id)
        {
            var entity = new UserService().GetSummary(id);
            if (entity == null) { return null; }

            return new MemberPrincipal
            {
                Identity = new MemberIdentity
                {
                    Id = entity.Id,
                    Name = entity.Name,
                    IsAuthenticated = true,
                    Roles = entity.IsAdmin ? new string[] { "admin" } : new string[0]
                }
            };
        }
    }

    public class MemberIdentity : IIdentity
    {
        public string AuthenticationType
        {
            get { return "internal"; }
        }

        public Guid Id { get; set; }

        public bool IsAuthenticated { get; set; }

        public string Name { get; set; }

        public string[] Roles { get; set; }
    }
}