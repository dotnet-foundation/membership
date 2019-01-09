using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Membership.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Newtonsoft.Json.Linq;

namespace Membership.Services
{
    public class UsersService
    {
        private readonly IGraphServiceClient _graphClient;

        private readonly IHttpContextAccessor _context;
        private readonly string _membersGroupId;

        public UsersService(IGraphApplicationClient graphClient, IHttpContextAccessor context, IOptions<AdminConfig> options)
        {
            _graphClient = graphClient;
            _context = context;
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

            var userRequest = _graphClient.Groups[_membersGroupId].Members.Request().Select("dotnetfoundation_member,givenName,surname,mail,otherMails,displayName");

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
            var user = await _graphClient.Users[id].Request().Select("dotnetfoundation_member,givenName,surname,mail,otherMails,displayName").GetAsync();

            return FromUser(user);
        }

        public async Task UpdateMemberAsync(string id, string displayName, bool? isActive, DateTimeOffset? expiration, string givenName, string surname, string githubId, string twitterId, string blogUrl)
        {
            if (string.IsNullOrWhiteSpace(displayName))
            {
                throw new ArgumentException("Argument cannot be blank when configured is true", nameof(displayName));
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

            var user = await _graphClient.Users[id].Request().UpdateAsync(toUpdate);


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

            if(user.AdditionalData?.ContainsKey("dotnetfoundation_member") == true)
            {
                var token = user.AdditionalData["dotnetfoundation_member"] as JToken;
                var ext = token.ToObject<MemberSchemaExtension>();
                member.TwitterId = ext.TwitterId;
                member.GitHubId = ext.GitHubId;
                member.BlogUrl = ext.BlogUrl;
                if(ext.IsActive.HasValue)
                    member.IsActive = ext.IsActive.Value;

                if(ext.ExpirationDateTime.HasValue)
                    member.Expiration = ext.ExpirationDateTime.Value;
            }

            return member;
        }
    }
}
