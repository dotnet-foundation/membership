using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Membership.Models;
using Membership.Services;
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
        private readonly UsersService _usersService;

        public ProfileController(UsersService usersService)
        {
            _usersService = usersService;
        }

        public async Task<IActionResult> Index()
        {
            var member = await _usersService.GetMe();

            var model = new UpdateMemberModel
            {
                Id = member.Id,
                GivenName = member.GivenName,
                Surname = member.Surname,
                DisplayName = member.DisplayName,
                GitHubId = member.GitHubId,
                Expiration = member.Expiration,
                IsActive = member.IsActive
            };

            return View(member);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, UpdateMemberModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var oid = User.FindFirst("oid").Value; // ignore any params and use the claims

                await _usersService.UpdateMemberAsync(oid, model.DisplayName, null, model.GivenName, model.Surname, model.GitHubId, null);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception e)
            {
                ModelState.TryAddModelError("", e.Message);
                return View(model);
            }
        }
    }
}
