using Domain.Common.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Common.Exceptions;

public class DomainException<TDomain> : Exception where TDomain : IEntity
{
    public string DomainName => nameof(TDomain);
    public DomainException(string message) : base(message) { }
    public DomainException(string message, TDomain entity) : base(message) { }
}
