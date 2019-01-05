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
        public async Task<IActionResult> Index()
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            var graphServiceClient = new GraphServiceClient(new DelegateAuthenticationProvider((requestMessage) =>
            {
                requestMessage
                    .Headers
                    .Authorization = new AuthenticationHeaderValue("bearer", accessToken);

                return Task.FromResult(0);
            }));
            graphServiceClient.BaseUrl = "https://graph.microsoft.com/beta";


            var user = await graphServiceClient.Me.Request().GetAsync();


            JArray otherMails = null;
            if (user.AdditionalData.ContainsKey("otherMails"))
            {
                otherMails = user.AdditionalData["otherMails"] as JArray;
            }
            
            var email = otherMails?.FirstOrDefault()?.Value<string>();


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
