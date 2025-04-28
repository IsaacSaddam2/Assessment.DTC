using SharedEventModels;

namespace UserRegistrationService
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = Host.CreateDefaultBuilder(args)
                .ConfigureServices((ctx, services) =>
                {
                    services.Configure<RabbitMqSettings>(ctx.Configuration.GetSection("RabbitMq"));
                    services.AddHostedService<ProcessUserRegistration>();
                })
                .Build();

            builder.Run();
        }
    }
}