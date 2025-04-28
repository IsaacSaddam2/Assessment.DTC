using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedEventModels
{
    public class UserNotificationSentEvent
    {
        public Guid UserId { get; init; }
        public DateTime SentAt { get; init; } = DateTime.UtcNow;
        public string NotificationType { get; init; } = null!;
    }
}
