using System;

namespace WorldsBestBars.Services.Email.MailChimp
{
    public static class Helper
    {
        public static void Subscribe(Guid id)
        {
            var service = new Subscribe();

            service.Execute(id);
        }

        public static void Unsubscribe(Guid id)
        {
            var service = new Unsubscribe();

            service.Execute(id);
        }
    }
}
