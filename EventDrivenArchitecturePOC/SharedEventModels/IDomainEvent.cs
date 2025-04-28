using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedEventModels
{
    public interface IDomainEvent
    {
        Guid AggregateId { get; }
        DateTime OccurredAt { get; }
    }
}
