namespace AIEngineGateway.Services
{
    using AIEngineConnectivity.Models;
    using AIEngineConnectivity.Repositories;
    using AIEngineConnectivity.Services;
    using Microsoft.Extensions.Options;
    using System.Net;
    using System.Net.Mail;
#nullable disable
    public class EmailService : IEmailService
    {
        private readonly SmtpConfiguration _smtpConfiguration;
        private readonly IUserService _userService;
        private readonly IRepositoryWrapper _repository;
        public EmailService(IOptions<SmtpConfiguration> options, IUserService userService, IRepositoryWrapper repositoryWrapper)
        {
            _smtpConfiguration = options.Value;
            _userService = userService;
            _repository = repositoryWrapper;
        }

        public async Task SendEmail(string to, string body)
        {
            try
            {
                using var client = new SmtpClient
                {
                    Host = _smtpConfiguration.Host,
                    Port = _smtpConfiguration.Port,
                    EnableSsl = _smtpConfiguration.EnableSSL,
                    Credentials = new NetworkCredential(_smtpConfiguration.User, _smtpConfiguration.Password)
                };

                var email = new MailMessage
                {
                    From = new MailAddress(_smtpConfiguration.User),
                    Body = body,
                    Subject = $"AI Engine Password Reset Confirmation",
                    IsBodyHtml = true,
                };

                email.To.Add(to);
                await client.SendMailAsync(email);
            }
            catch
            {
                throw;
            }
        }

        public async Task SendTestMail(SmtpConfiguration smtpConfiguration)
        {
            try
            {
                using var client = new SmtpClient
                {
                    Host = smtpConfiguration.Host,
                    Port = smtpConfiguration.Port,
                    EnableSsl = smtpConfiguration.EnableSSL,
                    Credentials = new NetworkCredential(smtpConfiguration.User, smtpConfiguration.Password)
                };

                var email = new MailMessage
                {
                    From = new MailAddress(smtpConfiguration.User),
                    Body = "Test Mail",
                    Subject = "Test Mail Send By AI Engine",
                    IsBodyHtml = true
                };

                var currentUserMail = await _repository.IdentityRepository.GetUserEmailById(_userService.GetCurrentUser.UserId);
                email.To.Add(currentUserMail);
                await client.SendMailAsync(email);
            }
            catch
            {
                throw;
            }
        }
    }
}
