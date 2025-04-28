using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using SharedEventModels;
using System.Text;

namespace EventPublisherApi.Controllers
{
    // EventDriven.API/Controllers/UsersController.cs
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly ConnectionFactory factory;
        private readonly RabbitMqSettings settings;
        private IConnection connection;
        private IChannel channel;

        public UsersController(IOptions<RabbitMqSettings> settings)
        {
            this.settings = settings.Value;

            factory = new ConnectionFactory
            {
                Uri = new Uri(this.settings.ConnectionUrl),
            };
        }

        [HttpPost]
        public async Task<IActionResult> RegisterUser([FromBody] UserRegistrationDto dto)
        {
            var userId = Guid.NewGuid();;

            connection = await factory.CreateConnectionAsync();
            channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(
                queue: this.settings.RegistrationQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);


            var message = new UserRegisteredEvent
            {
                UserId = new Guid(),
                Email = dto.Email,
                Name = dto.Name
            };

            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

            try
            {
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
            }
            catch (Exception ex)
            {
            }

            return Accepted(new { UserId = userId });
        }
    }

    public record UserRegistrationDto(string Email, string Name);
}
