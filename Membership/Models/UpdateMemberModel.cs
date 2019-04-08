using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Membership.Models
{
    public class UpdateMemberModel
    {
        [HiddenInput]
        public string Id { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Display Name is required")]
        [Display(Name = "Display Name")]
        public string DisplayName { get; set; }

        [Display(Name = "Given Name")]
        public string GivenName { get; set; }

        public string Surname { get; set; }

        public bool IsActive { get; set; }

        public DateTimeOffset Expiration { get; set; }

        [Display(Name = "GitHub ID")]
        public string GitHubId { get; set; }

        [Display(Name = "Twitter ID")]
        public string TwitterId { get; set; }

        [Display(Name = "Blog URL")]
        public string BlogUrl { get; set; }

        public int PhotoWidth { get; set; }
        public int PhotoHeight { get; set; }
        public string PhotoType { get; set; }
        public byte[] PhotoBytes { get; set; }

        [Display(Name = "Profile Photo (jpeg, max 100kb)")]
        public IFormFile PhotoUpload { get; set; }
    }
}
