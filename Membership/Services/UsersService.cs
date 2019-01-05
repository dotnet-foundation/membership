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

            // check email in two places: 1 Mail, 2 Other Mails
            var email = user.Mail ?? user.OtherMails?.FirstOrDefault();

            var member = new MemberModel()
            {
                DisplayName = user.DisplayName,
                Email = email,
                FirstName = user.GivenName,
                LastName = user.Surname
            };

            return member;
        }
    }
}
