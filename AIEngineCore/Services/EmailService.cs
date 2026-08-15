namespace AIEngineCore.Services
{
    using AIEngineConnectivity.EngineCore;
    using AIEngineConnectivity.Models;
    using AIEngineConnectivity.Repositories;
    using AIEngineConnectivity.Services;
    using AIEngineCore.EngineNotifications;
    using Microsoft.Extensions.Options;
    using System.Net;
    using System.Net.Mail;
#nullable disable

    public class EmailService : IEmailService
    {
        private readonly SmtpConfiguration _smtpConfiguration;
        private readonly IUserService _userService;
        private readonly IRepositoryWrapper _repository;
        private readonly ITemplateProvider _TemplateProvider;
        private readonly ITemplateRenderer _TemplateRender;

        public EmailService(IOptions<SmtpConfiguration> options, IUserService userService,
            IRepositoryWrapper repositoryWrapper, ITemplateProvider templateProvider,
            ITemplateRenderer templateRenderer)
        {
            _smtpConfiguration = options.Value;
            _userService = userService;
            _repository = repositoryWrapper;
            _TemplateProvider = templateProvider;
            _TemplateRender = templateRenderer;
        }

        public async Task SendEmail(EngineNotificationMessage notification, CancellationToken cancellation)
        {
            try
            {
                if (notification.Notification is not EngineEmailNotification EmailData)
                    throw new NotSupportedException("Internal Server Error");

                var rawTemplate = await _TemplateProvider.GetTemplate(notification.EngineEvents, cancellation);
                string renderedBody = _TemplateRender.Render(rawTemplate, EmailData.parameters);

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
                    Body = renderedBody,
                    Subject = EmailData.Subject,
                    IsBodyHtml = true,
                };

                email.To.Add(EmailData.ToAddress);
                await client.SendMailAsync(email, cancellation);
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