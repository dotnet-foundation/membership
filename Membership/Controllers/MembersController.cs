using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    }
}
