
namespace SharedEventModels
{
    public class RabbitMqSettings
    {
        public string ConnectionUrl { get; set; } = default!;
        public string RegistrationQueue { get; set; } = "user.registration";
        public string NotificationQueue { get; set; } = "user.notification";
        public string DeadLetterExchange { get; set; } = "dead-letter-exchange";
    }
}
