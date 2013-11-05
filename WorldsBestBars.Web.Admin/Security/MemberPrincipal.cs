using System;
using System.Linq;
using System.Security.Principal;
using WorldsBestBars.Services;

namespace WorldsBestBars.Web.Admin.Security
{
    /// <summary>
    /// 
    /// </summary>
    public class MemberPrincipal : IPrincipal
    {
        /// <summary>
        /// Gets the identity of the current principal.
        /// </summary>
        /// <returns>The <see cref="T:WorldsBestBars.Web.Admin.Security.MemberIdentity" /> object associated with the current principal.</returns>
        public IIdentity Identity { get; private set; }

        /// <summary>
        /// Determines whether the current principal belongs to the specified role.
        /// </summary>
        /// <param name="role">The name of the role for which to check membership.</param>
        /// <returns>
        /// true if the current principal is a member of the specified role; otherwise, false.
        /// </returns>
        public bool IsInRole(string role)
        {
            if (Identity is MemberIdentity)
            {
                return (Identity as MemberIdentity).Roles.Contains(role);
            }

            return false;
        }

        /// <summary>
        /// Gets the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
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

    /// <summary>
    /// 
    /// </summary>
    public class MemberIdentity : IIdentity
    {
        /// <summary>
        /// Gets the type of authentication used.
        /// </summary>
        /// <returns>The type of authentication used to identify the user.</returns>
        public string AuthenticationType
        {
            get { return "internal"; }
        }

        /// <summary>
        /// Gets or sets the identifier of the current user.
        /// </summary>
        /// <value>
        /// The database identifier of the current user.
        /// </value>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets a value that indicates whether the user has been authenticated.
        /// </summary>
        /// <returns>true if the user was authenticated; otherwise, false.</returns>
        public bool IsAuthenticated { get; set; }

        /// <summary>
        /// Gets the name of the current user.
        /// </summary>
        /// <returns>The name of the user on whose behalf the code is running.</returns>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the roles.
        /// </summary>
        /// <value>
        /// The roles.
        /// </value>
        public string[] Roles { get; set; }
    }
}