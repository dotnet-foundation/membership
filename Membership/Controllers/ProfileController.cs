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

            
            return View(member);
        }
    }
}
