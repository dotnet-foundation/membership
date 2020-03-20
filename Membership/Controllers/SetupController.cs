using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using Membership.Models;
using Membership.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Membership.Controllers
{
    [Authorize(Roles = "admin")]
    public class SetupController : Controller
    {
        private readonly UsersService _usersService;
        private readonly ILogger<SetupController> _logger;

        public SetupController(UsersService usersService, ILogger<SetupController> logger)
        {
            _usersService = usersService;
            _logger = logger;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> AddMembers()
        {

            //TODO Handle CSV upload rather than read from disk
            using (var reader = new StreamReader("MemberInvitation\\azure_ad_b2b.csv", Encoding.GetEncoding(1252)))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var members = csv.GetRecords<ImportMember>();


                foreach (var member in members)
                {
                    await _usersService.InviteMember($"{member.FirstName} {member.LastName}", member.FirstName, member.EMail);
                }
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> RegisterSchemaExtensions()
        {
            await Task.Delay(0);
            //var extensions = await _graphDelegatedClient.SchemaExtensions.Request().AddAsync(new SchemaExtension
            //{
            //    Id = "dotnetfoundation_member",
            //    Description = ".NET Foundation membership extension",
            //    TargetTypes = new[] { "User" },
            //    Properties = new []
            //    {
            //        new ExtensionSchemaProperty
            //        {
            //            Name = "isActive",
            //            Type = "Boolean"
            //        },
            //        new ExtensionSchemaProperty
            //        {
            //            Name = "expirationDateTime",
            //            Type = "DateTime"                        
            //        },
            //        new ExtensionSchemaProperty
            //        {
            //            Name = "githubId",
            //            Type = "String"
            //        },
            //        new ExtensionSchemaProperty
            //        {
            //            Name = "twitterId",
            //            Type = "String"
            //        },
            //        new ExtensionSchemaProperty
            //        {
            //            Name = "blogUrl",
            //            Type = "String"
            //        }
            //    }
            //});

            //   var ext = await _graphDelegatedClient.SchemaExtensions.Request().Filter("id eq 'dotnetfoundation_member'").GetAsync();


            return RedirectToAction(nameof(Index));
        }
    }
}
