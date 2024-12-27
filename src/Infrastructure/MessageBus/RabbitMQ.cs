
using MediatR;
using MassTransit;
using Application.Dtos;

namespace Infrastructure.MessageBus;


public class CreateEnergyLogConsumer(IMediator mediator) : IConsumer<CreateEnergyLogMessage>
{
    private readonly IMediator _mediator = mediator;

    public Task Consume(ConsumeContext<CreateEnergyLogMessage> context)
    {
        throw new NotImplementedException();
    }
}
