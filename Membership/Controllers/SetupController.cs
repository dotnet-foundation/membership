using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using Membership.Models;
using Membership.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;

namespace Membership.Controllers
{
    [Authorize(Roles = "admin")]
    public class SetupController : Controller
    {
        private readonly IGraphDelegatedClient _graphDelegatedClient;
        private readonly IGraphServiceClient _graphApplicationClient;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly ILogger<SetupController> _logger;

        public SetupController(IGraphDelegatedClient graphDelegatedClient, IGraphApplicationClient graphApplicationClient, IWebHostEnvironment hostingEnvironment, ILogger<SetupController> logger)
        {
            _graphDelegatedClient = graphDelegatedClient;
            _graphApplicationClient = graphApplicationClient;
            _hostingEnvironment = hostingEnvironment;
            _logger = logger;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> AddMembers()
        {
            const string emailSender = "jon@dotnetfoundation.org";
            const string emailSubject = ".NET Foundation: Please Activate Your Membership";
            const string redirectUrl = "https://members.dotnetfoundation.org/Profile";

            var path = Path.Combine(_hostingEnvironment.ContentRootPath, "MemberInvitation");
            var mailTemplate = await System.IO.File.ReadAllTextAsync(Path.Combine(path, "email-template.html"));
            var attachments = new MessageAttachmentsCollectionPage
            {
                await GetImageAttachement(path, "header.png"),
                await GetImageAttachement(path, "footer.png")
            };

            const string groupId = "940ac926-845c-489b-a270-eb961ca4ca8f"; //Members
            //const string groupId = "6eee9cd2-a055-433d-8ff1-07ca1d0f6fb7"; //Test Members
            //We can look up the Members group by name, but in this case it's constant
            //var group = await _graphApplicationClient
            //        .Groups.Request().Filter("startswith(displayName,'Members')")
            //        .GetAsync();
            //string groupId = group[0].Id;

            //TODO Handle CSV upload rather than read from disk
            using (var reader = new StreamReader("MemberInvitation\\azure_ad_b2b.csv", Encoding.GetEncoding(1252)))
            using (var csv = new CsvReader(reader))
            {
                var members = csv.GetRecords<ImportMember>();

                //If we wanted to check if members are in the group first, could use this
                //var existing = await _graphApplicationClient
                //        .Groups[groupId]
                //        .Members.Request().Select("id,mail").GetAsync();

                foreach (var member in members)
                {
                    var invite = new Invitation
                    {
                        InvitedUserEmailAddress = member.EMail.Trim(),
                        SendInvitationMessage = false,
                        InviteRedirectUrl = redirectUrl,
                        InvitedUserDisplayName = member.FirstName + " " + member.LastName
                    };

                    var result = await _graphApplicationClient.Invitations.Request().AddAsync(invite);

                    try
                    {
                        await _graphApplicationClient
                            .Groups[groupId]
                            .Members.References.Request()
                            .AddAsync(result.InvitedUser);
                    }
                    catch (Exception)
                    {
                        //They're already added to the group, so we can break without sending e-mail
                        _logger.LogWarning("User exists: {FirstName} {LastName}: {EMail}", member.FirstName, member.LastName, member.EMail);
                        continue;
                    }

                    var recipients = new List<Recipient>
                    {
                        new Recipient
                        {
                            EmailAddress = new EmailAddress
                            {
                                Name = member.FirstName + " " + member.LastName,
                                Address = member.EMail
                            }
                        }
                    };

                    // Create the message.
                    var email = new Message
                    {
                        Body = new ItemBody
                        {
                            //TODO Replace with e-mail template
                            Content = mailTemplate
                                .Replace("{{action}}", result.InviteRedeemUrl)
                                .Replace("{{name}}", member.FirstName),
                            ContentType = BodyType.Html,
                        },
                        HasAttachments = true,
                        Attachments = attachments,
                        Subject = emailSubject,
                        ToRecipients = recipients
                    };

                    // Send the message.
                    await _graphApplicationClient.Users[emailSender].SendMail(email, true).Request().PostAsync();
                    _logger.LogInformation("Invite: {FirstName} {LastName}: {EMail}", member.FirstName, member.LastName, member.EMail);
                }
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<Attachment> GetImageAttachement(string path, string filename)
        {
            var bytes = await System.IO.File.ReadAllBytesAsync(Path.Combine(path, filename));

            return new FileAttachment
            {
                ODataType = "#microsoft.graph.fileAttachment",
                ContentBytes = bytes,
                ContentType = "image/png",
                ContentId = filename,
                Name = filename
            };
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
