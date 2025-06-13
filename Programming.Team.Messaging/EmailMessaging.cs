using Azure.Communication.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Programming.Team.Core;
using Programming.Team.Data.Core;
using Programming.Team.Messaging.Core;
using Programming.Team.Templating.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Programming.Team.Messaging
{
    public class EmailMessaging : IEmailMessaging
    {
        protected EmailClient Client { get; }
        protected IUserRepository UserRepository { get; }
        protected IDocumentTemplator DocumentTemplator { get; }
        protected ILogger Logger { get; }
        protected string FromAddress { get; }
        protected string ReplyToAddress { get; }
        public EmailMessaging(EmailClient client, IUserRepository userRepository, 
            IDocumentTemplator documentTemplator, ILogger<EmailMessaging> logger, IConfiguration config) 
        {
            Client = client;
            UserRepository = userRepository;
            DocumentTemplator = documentTemplator;
            Logger = logger;
            FromAddress = config["ACS:FromAddress"] ?? throw new InvalidDataException("From address is required in configuration.");
            ReplyToAddress = config["ACS:ReplyToAddress"] ?? throw new InvalidDataException("Reply-to address is required in configuration.");
        }
        public virtual async Task SendEmails(string messageTemplate, string subjectTemplate, MessageType type = MessageType.PlainText, Guid[]? userIds = null, CancellationToken token = default)
        {
            try
            {
                await foreach(var user in GetUsers(userIds, token))
                {
                    if (string.IsNullOrWhiteSpace(user.EmailAddress))
                        continue;
                    var message = await DocumentTemplator.ApplyTemplate(messageTemplate, user, token: token);
                    var subject = await DocumentTemplator.ApplyTemplate(subjectTemplate, user, token: token);
                    var emailMessage = new EmailMessage(
                        senderAddress: FromAddress,
                         recipients: new EmailRecipients(new List<EmailAddress> { new EmailAddress(user.EmailAddress) }),
                        content: type == MessageType.PlainText ? new EmailContent(subject)
                        {
                            PlainText = message
                        } : new EmailContent(subject)
                        {
                            Html = message
                        }
                    );
                    emailMessage.ReplyTo.Add(new EmailAddress(ReplyToAddress));
                    await Client.SendAsync(Azure.WaitUntil.Started, emailMessage, token);
                    Logger.LogTrace("Email sent to {Email} with subject {Subject}", user.EmailAddress, subject);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error sending emails with template {MessageTemplate} and subject {SubjectTemplate}", messageTemplate, subjectTemplate);
                throw;
            }
        }
        protected async IAsyncEnumerable<User> GetUsers(Guid[]? userIds, [EnumeratorCancellation]CancellationToken token = default)
        {
            await using (var uow = UserRepository.CreateUnitOfWork())
            {
                if (userIds == null || userIds.Length == 0)
                {
                    int page = 1;
                    RepositoryResultSet<Guid, User>? results = null;
                    do
                    {
                        results = await UserRepository.Get(uow, new Pager() { Page = page, Size = 100 }, orderBy: q => q.OrderBy(e => e.Id), token: token);
                        foreach (var user in results.Entities)
                        {
                            yield return user;
                        }
                    }
                    while (results?.Page > page);
                }
                else
                {
                    foreach (var userId in userIds)
                    {
                        var user = await UserRepository.GetByID(userId, uow, token: token);
                        if (user != null)
                        {
                            yield return user;
                        }
                    }
                }
            }
        }
    }
}
