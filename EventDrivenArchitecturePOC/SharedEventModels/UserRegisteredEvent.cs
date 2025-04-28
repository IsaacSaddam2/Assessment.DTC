using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedEventModels
{
    public class UserRegisteredEvent
    {
        public Guid UserId { get; init; }
        public DateTime RegisteredAT { get; init; } = DateTime.UtcNow;
        public string Email { get; init; } = null!;
        public string Name { get; init; } = null!;
    }
}
