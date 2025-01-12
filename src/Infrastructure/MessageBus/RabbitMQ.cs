
using MediatR;
using MassTransit;
using Application.Dtos.Messages;
using Application.Commands;

namespace Infrastructure.MessageBus;


public class CreateEnergyLogConsumer(IMediator mediator) : IConsumer<CreateEnergyLogMessage>
{
    private readonly IMediator _mediator = mediator;

    public async Task Consume(ConsumeContext<CreateEnergyLogMessage> context) 
        => await _mediator.Send(new CreateEnergyLogCommand(context.Message.EnergyLog, context.Message.EquipmentId));
}


public class CreateEnergyLogConsumerDefinition : ConsumerDefinition<CreateEnergyLogConsumer>
{
    public CreateEnergyLogConsumerDefinition()
    {
        ConcurrentMessageLimit = 5;
    }

    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<CreateEnergyLogConsumer> consumerConfigurator)
    {
        base.ConfigureConsumer(endpointConfigurator, consumerConfigurator);

        endpointConfigurator.PrefetchCount = 15;
        endpointConfigurator.DiscardFaultedMessages();

    }
}
