using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Membership.Models
{
    public class UpdateMemberModel
    {
        [HiddenInput]
        public string Id { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Display Name is required")]
        public string DisplayName { get; set; }
        public string GivenName { get; set; }
        public string Surname { get; set; }
        public bool IsActive { get; set; }
        public DateTimeOffset Expiration { get; set; }
        public string GitHubId { get; set; }
    }
}
