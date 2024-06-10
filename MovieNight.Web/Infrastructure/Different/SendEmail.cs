using System.Net;
using System.Net.Mail;

namespace MovieNight.Web.Infrastructure.Different
{
    public static class SendEmail
    {
        /// <summary>
        /// I used to send messages my mail backup and service "mailtrap" to 
        /// check job. https://mailtrap.io/
        /// </summary>
        /// <param name="email"></param>
        /// <param name="code"></param>
        public static void SendRecoveryEmail(string email, string code)
        {
            var fromAddress = new MailAddress("movienightt81@gmail.com", "MovieNight");
            var toAddress = new MailAddress(email);
            const string fromPassword = "745eb4ef095036"; // Replace with your Mailtrap password
            const string subject = "Password Recovery Code";
            string body = $"Your recovery code is: {code}";

            var smtp = new SmtpClient
            {
                Host = "sandbox.smtp.mailtrap.io",
                Port = 2525,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("57016cc4c56211", fromPassword)
            };

            using (var message = new MailMessage(fromAddress, toAddress)
                   {
                       Subject = subject,
                       Body = body
                   })
            {
                smtp.Send(message);
            }
        }

    }
}