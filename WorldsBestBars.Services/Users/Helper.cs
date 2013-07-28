using System;
using WorldsBestBars.Services.Models;

namespace WorldsBestBars.Services.Users
{
    public static class Helper
    {
        public static UserSummary Get(Guid id)
        {
            var service = new Get();

            return service.Execute(id);
        }

        public static bool IsEmailInUse(string email)
        {
            var service = new IsEmailInUse();

            return service.Execute(email);
        }
    }
}
