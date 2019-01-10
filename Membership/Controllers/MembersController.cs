using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Membership.Models;
using Membership.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Membership.Controllers
{
    [Authorize(Roles = "admin")]
    public class MembersController : Controller
    {
        private readonly UsersService _usersService;

        public MembersController(UsersService usersService)
        {
            _usersService = usersService;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _usersService.GetAllMembers();

            return View(users);
        }
        
        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                var member = await _usersService.GetMemberById(id);

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
            catch (Exception e)
            {
                ModelState.TryAddModelError("", e.Message);
                return View(); // TODO: error message 
            }            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, UpdateMemberModel model)
        {
            if (model.PhotoUpload?.Length > 100 * 1024)
            {
                ModelState.AddModelError(nameof(model.PhotoUpload), "Image size must be less than 100kb");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                byte[] buffer = null;

                if (model.PhotoUpload?.Length > 0)
                {
                    using (var ms = new MemoryStream((int)model.PhotoUpload.Length))
                    {
                        await model.PhotoUpload.CopyToAsync(ms);
                        ms.Position = 0;
                        buffer = ms.ToArray();
                    }
                }

                await _usersService.UpdateMemberAsync(id, model.DisplayName, null, null, model.GivenName, model.Surname, model.GitHubId, model.TwitterId, model.BlogUrl, buffer);

                return RedirectToAction(nameof(Index));
            }
            catch(Exception e)
            {
                ModelState.TryAddModelError("", e.Message);
                return View(model); 
            }
        }
    }
}
