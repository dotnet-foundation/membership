using System;
using System.Collections.Generic;
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
                    Expiration = member.Expiration,
                    IsActive = member.IsActive
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
            if(!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _usersService.UpdateMemberAsync(id, model.DisplayName, model.IsActive, model.GivenName, model.Surname, model.GitHubId, model.Expiration);

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
