using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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
        public string TwitterId { get; set; }
        public string BlogUrl { get; set; }

        public int PhotoWidth { get; set; }
        public int PhotoHeight { get; set; }

        public string PhotoType { get; set; }

        public byte[] PhotoBytes { get; set; }

        [Display(Name = "Profile Photo (max 100kb)")]        
        public IFormFile PhotoUpload { get; set; }
    }
}
