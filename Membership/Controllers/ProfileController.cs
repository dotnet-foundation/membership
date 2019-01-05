using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Membership.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using Newtonsoft.Json.Linq;

namespace Membership.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IGraphServiceClient _graphClient;

        public ProfileController(IGraphServiceClient graphClient)
        {
            _graphClient = graphClient;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _graphClient.Me.Request().GetAsync();


            // check email in two places: 1 Mail, 2 Other Mails
            var email = user.Mail ?? user.OtherMails?.FirstOrDefault();

            var member = new MemberModel()
            {
                DisplayName = user.DisplayName,
                Email = email,
                FirstName = user.GivenName,
                LastName = user.Surname
            };

            return View(member);
        }
    }
}
