using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Membership.Models
{
    public class MemberModel
    {
        public string DisplayName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsActive { get; set; }
        public DateTimeOffset Expiration { get; set; }
        public string Email { get; set; }
        public string GitHubId { get; set; }
    }
}
