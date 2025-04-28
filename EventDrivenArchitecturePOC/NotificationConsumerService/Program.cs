using SharedEventModels;

namespace NotificationConsumerService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateDefaultBuilder(args)
                .ConfigureServices((ctx, services) =>
                {
                    services.Configure<RabbitMqSettings>(ctx.Configuration.GetSection("RabbitMq"));
                    services.AddHostedService<ProcessNotification>();
                })
                .Build();

            builder.Run();
        }
    }
}