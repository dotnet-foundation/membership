using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Membership.Models;
using Microsoft.Graph;

namespace Membership.Services
{
    public class UsersService
    {
        private readonly IGraphServiceClient _graphClient;

        public UsersService(IGraphServiceClient graphClient)
        {
            _graphClient = graphClient;
        }

        // Get the current user's profile.
        public async Task<MemberModel> GetMe()
        {
            // Get the current user's profile.
            var user = await _graphClient.Me.Request().GetAsync();

            return FromUser(user);
        }

        public async Task<List<MemberModel>> GetAllMembers()
        {
            var items = new List<MemberModel>();

            // Get users.
            var userRequest = _graphClient.Users.Request();//.Filter("userType eq 'Guest'");

            do
            {
                var users = await userRequest.GetAsync();
                foreach (var user in users)
                {
                    items.Add(FromUser(user));
                }

                userRequest = users.NextPageRequest;
            } while (userRequest != null);

            // Graph can't sort by surname?

            items = items.OrderBy(m => m.LastName).ToList();

            return items;
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
                FirstName = user.GivenName,
                LastName = user.Surname
            };

            return member;
        }
    }
}
