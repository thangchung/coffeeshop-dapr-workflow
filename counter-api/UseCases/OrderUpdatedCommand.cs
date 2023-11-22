using Dapr.Client;
using FluentValidation;
using MediatR;
using Newtonsoft.Json;

using CounterApi.Domain.Dtos;
using CounterApi.Domain.Messages;

namespace CounterApi.UseCases;

public static class OrderUpRouteMapper
{
    public static IEndpointRouteBuilder MapOrderUpApiRoutes(this IEndpointRouteBuilder builder)
    {
        builder.MapSubscribeHandler();

        var baristaOrderUpTopic = new Dapr.TopicOptions
        {
            PubsubName = "baristapubsub",
            Name = "baristaorderupdated",
            DeadLetterTopic = "orderupDeadLetterTopic"
        };

        var kitchenOrderUpTopic = new Dapr.TopicOptions
        {
            PubsubName = "kitchenpubsub",
            Name = "kitchenorderupdated",
            DeadLetterTopic = "orderupDeadLetterTopic"
        };

        builder.MapPost("/dapr_subscribe_BaristaOrderUpdated", async (BaristaOrderUpdated @event, ISender sender) =>
            await sender.Send(new OrderUpdatedCommand(
                    @event.OrderId,
                    @event.ItemLines)))
                .WithTopic(baristaOrderUpTopic);

        builder.MapPost("/dapr_subscribe_KitchenOrderUpdated", async (KitchenOrderUpdated @event, ISender sender) =>
            await sender.Send(new OrderUpdatedCommand(
                    @event.OrderId,
                    @event.ItemLines,
                    IsBarista: false)))
                .WithTopic(kitchenOrderUpTopic);

        return builder;
    }
}

public record OrderUpdatedCommand(Guid OrderId, List<OrderItemLineDto> ItemLines, bool IsBarista = true) : IRequest<IResult>;

internal class OrderUpdatedCommandValidator : AbstractValidator<OrderUpdatedCommand>
{
}

internal class OrderUpdatedCommandHandler(DaprClient daprClient, ILogger<OrderUpdatedCommandHandler> logger) : IRequestHandler<OrderUpdatedCommand, IResult>
{
    public async Task<IResult> Handle(OrderUpdatedCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("OrderUpdatedCommand received: {OrderUpdatedCommand}",
            JsonConvert.SerializeObject(request));

        if (request.IsBarista)
        {
            await daprClient.RaiseWorkflowEventAsync(
                request.OrderId.ToString(),
                "dapr",
                "BaristaOrderUpdated",
                request,
                cancellationToken
            );
        }
        else
        {
            await daprClient.RaiseWorkflowEventAsync(
                request.OrderId.ToString(),
                "dapr",
                "KitchenOrderUpdated",
                request,
                cancellationToken
            );
        }

        return Results.Ok();
    }
}