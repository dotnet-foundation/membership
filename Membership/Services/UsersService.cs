using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Membership.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Newtonsoft.Json.Linq;

namespace Membership.Services
{
    public class UsersService
    {
        private readonly IGraphServiceClient _graphApplicationClient;
        private readonly IHttpContextAccessor _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<UsersService> _logger;
        private readonly string _membersGroupId;

        public UsersService(IGraphApplicationClient graphClient, IHttpContextAccessor context, IWebHostEnvironment webHostEnvironment, IOptions<AdminConfig> options, ILogger<UsersService> logger)
        {
            _graphApplicationClient = graphClient;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
            _membersGroupId = options.Value.MembersGroupId;
        }

        // Get the current user's profile.
        public async Task<MemberModel> GetMe()
        {
            // Get the current user's profile.
            var oid = _context.HttpContext.User.FindFirst("oid").Value;

            return await GetMemberById(oid);
        }

        public async Task<List<MemberModel>> GetAllMembers()
        {
            var items = new List<MemberModel>();

            // Get users in the group

            var userRequest = _graphApplicationClient.Groups[_membersGroupId].Members.Request().Select("dotnetfoundation_member,id,givenName,surname,mail,otherMails,displayName");

            do
            {
                var users = await userRequest.GetAsync();
                foreach (User user in users)
                {
                    items.Add(FromUser(user));
                }

                userRequest = users.NextPageRequest;
            } while (userRequest != null);

            // Graph can't sort by surname?

            items = items.OrderBy(m => m.Surname).ToList();

            return items;
        }

        public async Task<MemberModel> GetMemberById(string id)
        {
            var user = await _graphApplicationClient.Users[id].Request().Select("dotnetfoundation_member,id,givenName,surname,mail,otherMails,displayName").GetAsync();

            try
            {
                var photo = await _graphApplicationClient.Users[id].Photo.Request().GetAsync();
                user.Photo = photo;

                // got a photo, now get the contents
                // Get my photo.
                using (var photoStream = await _graphApplicationClient.Users[id].Photo.Content.Request().GetAsync())
                {
                    if (photoStream != null)
                    {

                        // Get byte[] for display.
                        using (var reader = new BinaryReader(photoStream))
                        {
                            var data = reader.ReadBytes((int)photoStream.Length);
                            user.Photo.AdditionalData["data"] = data;
                        }
                    }
                }

            }
            catch (ServiceException)
            {
                // not present
            }

            return FromUser(user);
        }

        public async Task UpdateMemberAsync(string id, string displayName, bool? isActive, DateTimeOffset? expiration, string givenName, string surname, string githubId, string twitterId, string blogUrl, byte[] profilePhoto)
        {
            if (string.IsNullOrWhiteSpace(displayName))
            {
                throw new ArgumentException("Argument cannot be blank when configured is true", nameof(displayName));
            }

            // If we have a photo, check if it's a jpeg
            if (profilePhoto != null)
            {
                if (profilePhoto.Length <= 4 || !HasJpegHeader(profilePhoto))
                {
                    throw new ArgumentException("Profile photo is not a jpeg.", nameof(displayName));
                }
            }

            var extensionInstance = new Dictionary<string, object>
            {
                { "dotnetfoundation_member", new MemberSchemaExtension
                {
                    GitHubId = githubId,
                    TwitterId = twitterId,
                    BlogUrl = blogUrl,
                    IsActive = isActive,
                    ExpirationDateTime = expiration
                } }
            };

            var toUpdate = new User
            {
                DisplayName = displayName,
                GivenName = givenName,
                Surname = surname,
                AdditionalData = extensionInstance
            };

            var user = await _graphApplicationClient.Users[id].Request().UpdateAsync(toUpdate);

            if (profilePhoto != null && profilePhoto.Length > 0)
            {
                using (var ms = new MemoryStream(profilePhoto))
                {
                    await _graphApplicationClient.Users[id].Photo.Content.Request().PutAsync(ms);
                }
            }
        }

        public async Task UpdateMemberActiveAsync(string id, bool isActive)
        {
            var extensionInstance = new Dictionary<string, object>
            {
                { "dotnetfoundation_member", new MemberSchemaExtension
                    {
                        IsActive = isActive,
                        ExpirationDateTime = isActive ? DateTimeOffset.UtcNow.AddYears(1) : DateTimeOffset.UtcNow
                    }
                }
            };

            var toUpdate = new User
            {
                AdditionalData = extensionInstance
            };

            //TODO: This is causing updates to fail
            //if (isActive)
            //{
            //    toUpdate.HireDate = DateTimeOffset.UtcNow;
            //}

            var user = await _graphApplicationClient.Users[id].Request().UpdateAsync(toUpdate);
        }


        public async Task<User> InviteMember(string displayName, string firstName, string emailAddress)
        {

            const string emailSender = "jon@dotnetfoundation.org";
            const string emailSubject = ".NET Foundation: Please Activate Your Membership";
            const string redirectUrl = "https://members.dotnetfoundation.org/Profile";

            var path = Path.Combine(_webHostEnvironment.ContentRootPath, "MemberInvitation");
            var mailTemplate = await System.IO.File.ReadAllTextAsync(Path.Combine(path, "email-template.html"));
            var attachments = new MessageAttachmentsCollectionPage
            {
                await GetImageAttachement(path, "header.png"),
                await GetImageAttachement(path, "footer.png")
            };

            //const string groupId = "6eee9cd2-a055-433d-8ff1-07ca1d0f6fb7"; //Test Members
            //We can look up the Members group by name, but in this case it's constant
            //var group = await _graphApplicationClient
            //        .Groups.Request().Filter("startswith(displayName,'Members')")
            //        .GetAsync();
            //string groupId = group[0].Id;

            string redeemUrl;
            User invitedUser;
            try
            {

                //If we wanted to check if members are in the group first, could use this
                //var existing = await _graphApplicationClient
                //        .Groups[groupId]
                //        .Members.Request().Select("id,mail").GetAsync();

                var invite = new Invitation
                {
                    InvitedUserEmailAddress = emailAddress,
                    SendInvitationMessage = false,
                    InviteRedirectUrl = redirectUrl,
                    InvitedUserDisplayName = displayName
                };

                var result = await _graphApplicationClient.Invitations.Request().AddAsync(invite);

                redeemUrl = result.InviteRedeemUrl;
                invitedUser = result.InvitedUser;

                // Add to member group
                await _graphApplicationClient
                    .Groups[Constants.MembersGroupId]
                    .Members.References.Request()
                    .AddAsync(result.InvitedUser);
            }
            catch (Exception)
            {
                //They're already added to the group, so we can break without sending e-mail
                _logger.LogWarning("User exists: {DisplayName}: {EMail}", displayName, emailAddress);
                return null;
            }

            var recipients = new List<Recipient>
                    {
                        new Recipient
                        {
                            EmailAddress = new EmailAddress
                            {
                                Name = displayName,
                                Address = emailAddress
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
                        .Replace("{{action}}", redeemUrl)
                        .Replace("{{name}}", firstName),
                    ContentType = BodyType.Html,
                },
                HasAttachments = true,
                Attachments = attachments,
                Subject = emailSubject,
                ToRecipients = recipients
            };

            // Send the message.
            await _graphApplicationClient.Users[emailSender].SendMail(email, true).Request().PostAsync();
            _logger.LogInformation("Invite: {DisplayName}: {EMail}", displayName, emailAddress);

            return invitedUser;
        }

        public async Task<string> ChangeMemberLogonAddress(string id, string newAddress)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("message", nameof(id));
            if (string.IsNullOrWhiteSpace(newAddress))
                throw new ArgumentException("message", nameof(newAddress));

            // This method needs to
            // - invite a user at the new address
            // - Copy the relevant data to the new user
            // - delete the current user

            var user = await GetMemberById(id);

            var invitedUser = await InviteMember(user.DisplayName, user.GivenName, newAddress);

            if(invitedUser == null)
            {
                _logger.LogError("User {EMail} already exists, cannot copy data", newAddress);

                throw new Exception($"User {newAddress} already exists, cannot copy data");                
            }

            await UpdateMemberAsync(invitedUser.Id, user.DisplayName, user.IsActive, user.Expiration, user.GivenName, user.Surname, user.GitHubId, user.TwitterId, user.BlogUrl, user.PhotoBytes);

            // Copy any groups aside from the Members group
            var groupRequest = await _graphApplicationClient.Users[id].GetMemberGroups(securityEnabledOnly: false).Request().PostAsync();
            var groups = new List<string>();
            groups.AddRange(groupRequest.CurrentPage);
            {
                while(groupRequest.NextPageRequest != null)
                {
                    groupRequest = await groupRequest.NextPageRequest.PostAsync();
                    groups.AddRange(groupRequest.CurrentPage);
                }
            }
            var set = groups.ToHashSet(StringComparer.OrdinalIgnoreCase);
            set.Remove(Constants.MembersGroupId);

            // Add the user to the groups
            foreach(var gid in set)
            {
                await _graphApplicationClient
                    .Groups[gid]
                    .Members.References.Request()
                    .AddAsync(invitedUser);

                _logger.LogInformation("Added old user {user} to group {gid}", newAddress, gid);
            }


            // Finally, delete the old user

            await _graphApplicationClient.Users[id].Request().DeleteAsync();
            _logger.LogInformation("Removed old user {id}", id);

            return invitedUser.Id;
        }

        private static MemberModel FromUser(User user)
        {
            // check email in two places: 1 Mail, 2 Other Mailss
            var email = user.Mail ?? user.OtherMails?.FirstOrDefault();

            var member = new MemberModel()
            {
                Id = user.Id,
                DisplayName = user.DisplayName,
                Email = email,
                GivenName = user.GivenName,
                Surname = user.Surname
            };

            if (user.AdditionalData?.ContainsKey("dotnetfoundation_member") == true)
            {
                var token = user.AdditionalData["dotnetfoundation_member"] as JToken;
                var ext = token.ToObject<MemberSchemaExtension>();
                member.TwitterId = ext.TwitterId;
                member.GitHubId = ext.GitHubId;
                member.BlogUrl = ext.BlogUrl;
                member.IsActive = ext.IsActive;
                member.Expiration = ext.ExpirationDateTime;
            }

            if (user.Photo != null && user.Photo.AdditionalData.ContainsKey("data"))
            {
                member.PhotoHeight = user.Photo.Height.GetValueOrDefault();
                member.PhotoWidth = user.Photo.Width.GetValueOrDefault();
                member.PhotoType = user.Photo.AdditionalData["@odata.mediaContentType"] as string;
                member.PhotoBytes = user.Photo.AdditionalData["data"] as byte[];
            }

            return member;
        }

        private static bool HasJpegHeader(byte[] file)
        {
            var soi = BitConverter.ToUInt16(file);  // Start of Image (SOI) marker (FFD8)
            var marker = BitConverter.ToUInt16(file, 2); // JFIF marker (FFE0) or EXIF marker(FF01)

            return soi == 0xd8ff && (marker & 0xe0ff) == 0xe0ff;

        }

        private async Task<Attachment> GetImageAttachement(string path, string filename)
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
    }
}
