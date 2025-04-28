using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedEventModels
{
    public interface IMessageBus
    {
        Task PublishAsync<T>(T message, string routingKey);
        void Subscribe<T>(string queueName, Func<T, Task> handler);
    }
}
