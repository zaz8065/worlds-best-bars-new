namespace WorldsBestBars.Services.Users
{
    public static class Helper
    {
        public static bool IsEmailInUse(string email)
        {
            var service = new IsEmailInUse();

            return service.Execute(email);
        }
    }
}
