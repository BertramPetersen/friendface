using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace FriendFace._2FA;

public class EmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;
    public EmailSender(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public async Task SendEmailAsync(string email, string subject, string message)
    {
        var mailMessage = new MailMessage
        {
            From = new MailAddress(_configuration["EmailSettings:Sender"], _configuration["EmailSettings:SenderName"]),
            Subject = subject,
            Body = message,
            IsBodyHtml = true
        };
        mailMessage.To.Add(new MailAddress(email));

        using (var client = new SmtpClient(_configuration["EmailSettings:MailServer"], int.Parse(_configuration["EmailSettings:MailPort"])))
        {
            client.Credentials = new NetworkCredential(_configuration["EmailSettings:Sender"], _configuration["EmailSettings:Password"]);
            client.EnableSsl = true;
            await client.SendMailAsync(mailMessage);
        }
    }
}