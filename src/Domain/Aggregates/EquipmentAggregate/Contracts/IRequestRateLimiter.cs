using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Aggregates.EquipmentAggregate.Contracts;

public interface IRequestRateLimiter
{
    bool CanProcessRequest(string clientId);

}
