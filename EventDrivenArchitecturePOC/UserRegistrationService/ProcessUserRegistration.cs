using Azure.Core;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SharedEventModels;
using System.Text;
namespace UserRegistrationService
{
    public class ProcessUserRegistration : BackgroundService, IAsyncDisposable
    {
        private readonly ILogger<ProcessUserRegistration> _logger;
        private readonly RabbitMqSettings settings;
        private readonly ConnectionFactory factory;
        private IConnection connection;
        private IChannel channel;

        public ProcessUserRegistration(IOptions<RabbitMqSettings> settings, ILogger<ProcessUserRegistration> logger)
        {
            _logger = logger;
            this.settings = settings.Value;
            factory = new ConnectionFactory
            {
                Uri = new Uri(this.settings.ConnectionUrl)
            };
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                    await IntializeRabbitMq();

                    var consumer = new AsyncEventingBasicConsumer(channel);

                    consumer.ReceivedAsync += async (sender, eventArgs) =>
                    {
                        var body = eventArgs.Body.ToArray();

                        var message = JsonConvert.DeserializeObject<UserRegisteredEvent>(Encoding.UTF8.GetString(body));

                        _logger.LogInformation($"User is registered {message?.Name}");

                        try
                        {
                            await channel.BasicPublishAsync(
                                exchange: string.Empty,
                                routingKey: this.settings.NotificationQueue,
                                mandatory: true,
                                basicProperties: new BasicProperties
                                {
                                    ContentType = ContentType.ApplicationJson.ToString(),
                                    Persistent = true
                                },
                                body: body);

                        }
                        catch (Exception ex)
                        {
                        }

                        await channel.BasicAckAsync(eventArgs.DeliveryTag, false);

                    };

                    await channel.BasicConsumeAsync(settings.RegistrationQueue, autoAck: false, consumer);
                }
                await Task.Delay(1000, stoppingToken);
            }
        }

        public async ValueTask DisposeAsync()
        {
            await channel.DisposeAsync();
            await connection.DisposeAsync();
        }
        private async Task IntializeRabbitMq()
        {
            connection = await factory.CreateConnectionAsync();
            channel = await connection.CreateChannelAsync();

            // declare queues
            await channel.QueueDeclareAsync(
                queue: settings.RegistrationQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            await channel.QueueDeclareAsync(
                queue: settings.NotificationQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);
        }
    }
}
