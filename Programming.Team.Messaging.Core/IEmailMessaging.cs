using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Programming.Team.Messaging.Core
{
    public enum MessageType
    {
        Html,
        PlainText
    }
    public interface IEmailMessaging
    {
        Task SendEmails(string messageTemplate, string subjectTemplate, MessageType type = MessageType.PlainText, Guid[]? userIds = null, CancellationToken token = default);
    }
}
