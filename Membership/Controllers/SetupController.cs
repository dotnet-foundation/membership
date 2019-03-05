using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using Membership.Models;
using Membership.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Graph;

namespace Membership.Controllers
{
    [Authorize(Roles = "admin")]
    public class SetupController : Controller
    {
        private readonly IGraphDelegatedClient _graphDelegatedClient;
        private readonly IGraphServiceClient _graphApplicationClient;

        public SetupController(IGraphDelegatedClient graphDelegatedClient, IGraphApplicationClient graphApplicationClient)
        {
            _graphDelegatedClient = graphDelegatedClient;
            _graphApplicationClient = graphApplicationClient;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> AddMembers()
        {
            //TODO Handle CSV upload rather than read from disk
            using (var reader = new StreamReader("azure_ad_b2b.csv"))
            using (var csv = new CsvReader(reader))
            {
                var members = csv.GetRecords<ImportMember>();
                foreach (var member in members)
                {
                    Invitation invite = new Invitation();
                    invite.InvitedUserEmailAddress = member.EMail;
                    invite.SendInvitationMessage = false;
                    invite.InviteRedirectUrl = "https://members.dotnetfoundation.org/Profile";
                    invite.InvitedUserDisplayName = member.FirstName + " " + member.LastName;

                    var result = await _graphApplicationClient.Invitations.Request().AddAsync(invite);

                    await _graphApplicationClient
                        .Groups["6eee9cd2-a055-433d-8ff1-07ca1d0f6fb7"]
                        .Members.References.Request()
                        .AddAsync(result.InvitedUser);

                    List<Recipient> recipients = new List<Recipient>();
                    recipients.Add(new Recipient
                    {
                        EmailAddress = new EmailAddress
                        {
                            Address = member.EMail
                        }
                    });

                    // Create the message.
                    Message email = new Message
                    {
                        Body = new ItemBody
                        {
                            //TODO Replace with e-mail template
                            Content = $"<h1>Welcome to the .NET Foundation</h1><p><a href='{result.InviteRedeemUrl}'>click here to join</a></p>",
                            ContentType = BodyType.Text,
                        },
                        Subject = "Subject",
                        ToRecipients = recipients
                    };

                    // Send the message.
                    await _graphApplicationClient.Me.SendMail(email, true).Request().PostAsync();
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
