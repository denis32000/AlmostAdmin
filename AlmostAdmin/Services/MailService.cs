using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlmostAdmin.Services
{
    public class EmailService
    {
        public async void SendEmailAsync(string emailToWhom, string subject, string message, string nameFromWhom = "Almost Admin")
        {
            var emailMessage = new MimeMessage();
            var emailFromWhom = "postmaster@bitco.in.ua";

            emailMessage.From.Add(new MailboxAddress(nameFromWhom, emailFromWhom));
            emailMessage.To.Add(new MailboxAddress("", emailToWhom));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = message
            };

            using (var client = new SmtpClient())
            {
                try
                {
                    await client.ConnectAsync("mail.bitco.in.ua", 25, false);
                    await client.AuthenticateAsync(emailFromWhom, "3c4D*");
                    await client.SendAsync(emailMessage);

                    await client.DisconnectAsync(true);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
    }
}
