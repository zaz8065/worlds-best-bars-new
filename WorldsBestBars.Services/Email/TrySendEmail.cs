using System.Net.Mail;

namespace WorldsBestBars.Services.Email
{
    public class TrySendEmail : BaseService
    {
        #region Public Methods

        public void Execute(string sender, string recipient, string subject, string body)
        {
            var message = new MailMessage(sender, recipient, subject, body);
            message.IsBodyHtml = body.Contains("<html");

            using (var client = new SmtpClient())
            {
                client.Send(message);
            }
        }

        #endregion
    }
}
