using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Membership.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;

namespace Membership.Controllers
{
    [Authorize(Roles = "admin")]
    public class SetupController : Controller
    {
        private readonly IGraphDelegatedClient _graphDelegatedClient;

        public SetupController(IGraphDelegatedClient graphDelegatedClient)
        {
            _graphDelegatedClient = graphDelegatedClient;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> RegisterSchemaExtensions()
        {
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
