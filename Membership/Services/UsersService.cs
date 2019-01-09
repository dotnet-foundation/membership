using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Membership.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Graph;

namespace Membership.Services
{
    public class UsersService
    {
        private readonly IGraphServiceClient _graphClient;

        private readonly IHttpContextAccessor _context;
        private readonly string _membersGroupId;

        public UsersService(IGraphServiceClient graphClient, IHttpContextAccessor context, IOptions<AdminConfig> options)
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
        
            var userRequest = _graphClient.Groups[_membersGroupId].Members.Request();

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
            var user = await _graphClient.Users[id].Request().GetAsync();

            return FromUser(user);
        }

        public async Task UpdateMemberAsync(string id, string displayName, bool? isActive, string givenName, string surname, string githubId, DateTimeOffset? expiration)
        {
            if (string.IsNullOrWhiteSpace(displayName))
            {
                throw new ArgumentException("Argument cannot be blank when configured is true", nameof(displayName));
            }

            var user = await _graphClient.Users[id].Request().UpdateAsync(new User
            {
                DisplayName = displayName,
                GivenName = givenName,
                Surname = surname
            });
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

            return member;
        }
    }
}
