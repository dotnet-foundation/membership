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
                TwitterId = member.TwitterId,
                BlogUrl = member.BlogUrl,
                Expiration = member.Expiration,
                IsActive = member.IsActive,
                PhotoHeight = member.PhotoHeight,
                PhotoWidth = member.PhotoWidth,
                PhotoType = member.PhotoType,
                PhotoBytes = member.PhotoBytes
            };
            

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, UpdateMemberModel model)
        {

            if (model.PhotoUpload.Length > 100 * 1024)
            {
                ModelState.AddModelError(nameof(model.PhotoUpload), "Image size must be less than 100kb");
            }

            if (!ModelState.IsValid)
            {
                return View(nameof(Index), model);
            }


            try
            {
                var oid = User.FindFirst("oid").Value; // ignore any params and use the claims

                byte[] buffer = null;

                if(model.PhotoUpload.Length > 0)
                {
                    using (var ms = new MemoryStream((int)model.PhotoUpload.Length))
                    {
                        await model.PhotoUpload.CopyToAsync(ms);
                        ms.Position = 0;
                        buffer = ms.ToArray();
                    }
                }                    

                await _usersService.UpdateMemberAsync(oid, model.DisplayName, null, null, model.GivenName, model.Surname, model.GitHubId, model.TwitterId, model.BlogUrl, buffer);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception e)
            {
                ModelState.TryAddModelError("", e.Message);
                return View(nameof(Index), model);
            }
        }
    }
}
