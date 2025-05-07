using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using SharedEventModels;
using System.Text;
using Polly;
using Polly.Retry;

namespace EventPublisherApi.Controllers
{
    // EventDriven.API/Controllers/UsersController.cs
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly ILogger<UsersController> _logger;
        private readonly ConnectionFactory factory;
        private readonly RabbitMqSettings settings;
        private IConnection connection;
        private IChannel channel;
        private AsyncRetryPolicy policy;

        public UsersController(IOptions<RabbitMqSettings> settings, ILogger<UsersController> logger)
        {
            this.settings = settings.Value;

            factory = new ConnectionFactory
            {
                Uri = new Uri(this.settings.ConnectionUrl),
            };
            _logger = logger;
        }

        [HttpPost]
    public async Task<IActionResult> RegisterUser([FromBody] UserRegistrationDto dto)
        {
            await InitializeRabbitMqWithRetryPolicy();

            var userId = Guid.NewGuid();

            var message = new UserRegisteredEvent
            {
                UserId = userId, // fix: you should use the same userId here
                Email = dto.Email,
                Name = dto.Name
            };

            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));


            try
            {
                await policy.ExecuteAsync(async () =>
                {
                    // throw new Exception(); // uncomment this line to test retry mechanism
                    await channel.BasicPublishAsync(
                        exchange: string.Empty,
                        routingKey: this.settings.RegistrationQueue,
                        mandatory: true,
                        basicProperties: new BasicProperties
                        {
                            ContentType = "application/json",
                            Persistent = true
                        },
                        body: body);
                });
            }
            catch (Exception ex)
            {
                // Final catch after retries
                Console.WriteLine($"Failed to publish message after retries: {ex.Message}");
                return StatusCode(500, "Failed to publish the message");
            }

            return Accepted(new { UserId = userId });
        }

        private async Task InitializeRabbitMqWithRetryPolicy()
        {
            connection = await factory.CreateConnectionAsync();
            channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(
                queue: this.settings.RegistrationQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            policy = Policy
                .Handle<Exception>() // Retry on any exception during publish
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)), // Exponential backoff
                    onRetry: (exception, timespan, retryCount, context) =>
                    {
                        Console.WriteLine($"Retry {retryCount} after {timespan.TotalSeconds}s due to {exception.Message}");
                    });
        }
    }

public record UserRegistrationDto(string Email, string Name);
}
