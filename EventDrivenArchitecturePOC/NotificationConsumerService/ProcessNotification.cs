using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SharedEventModels;
using System.Text;
using System.Threading.Channels;

namespace NotificationConsumerService
{
    public class ProcessNotification : BackgroundService, IAsyncDisposable
    {
        private readonly ILogger<ProcessNotification> _logger;
        private readonly RabbitMqSettings settings;
        private readonly ConnectionFactory factory;
        private IConnection connection;
        private IChannel channel;

        public ProcessNotification(IOptions<RabbitMqSettings> settings, ILogger<ProcessNotification> logger)
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
                    connection = await factory.CreateConnectionAsync();
                    channel = await connection.CreateChannelAsync();

                    // should be coming from appsetings

                    await channel.QueueDeclareAsync(
                        queue: this.settings.NotificationQueue,
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);

                    var consumer = new AsyncEventingBasicConsumer(channel);

                    consumer.ReceivedAsync += async (sender, eventArgs) =>
                    {
                        var body = eventArgs.Body.ToArray();

                        var message = JsonConvert.DeserializeObject<UserRegisteredEvent>(Encoding.UTF8.GetString(body));
                        _logger.LogInformation($"Notification send to {message?.Email}");
                        await channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false);
                    };

                    await channel.BasicConsumeAsync(this.settings.NotificationQueue, autoAck: false, consumer);
                    await Task.Delay(1000, stoppingToken);
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            await channel.DisposeAsync();
            await connection.DisposeAsync();
        }
    }
}
