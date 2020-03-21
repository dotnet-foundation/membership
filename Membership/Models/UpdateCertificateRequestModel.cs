using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Membership.Models
{
    public class AddMembersRequestModel
    {

        [Display(Name = "Membership File (CSV)")]
        [Required]
        public IFormFile CsvFile { get; set; }
    }
}
