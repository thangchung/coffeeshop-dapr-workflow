using KitchenApi.Domain;
using KitchenApi.Domain.Dtos;
using KitchenApi.Domain.Messages;
using KitchenApi.Domain.SharedKernel;
using KitchenApi.Domain.DomainEvents;

using FluentValidation;
using MediatR;
using Newtonsoft.Json;
using Dapr.Client;

namespace KitchenApi.UseCases;

public static class OrderOrderedRouteMapper
{
    public static IEndpointRouteBuilder MapOrderUpApiRoutes(this IEndpointRouteBuilder builder)
    {
        builder.MapSubscribeHandler();

        var kitchenOrderedTopic = new Dapr.TopicOptions
        {
            PubsubName = "kitchenpubsub",
            Name = "kitchenordered",
            DeadLetterTopic = "kitchenorderedDeadLetterTopic"
        };

        builder.MapPost("/dapr_subscribe_KitchenOrdered", async (KitchenOrderPlaced @event, ISender sender) =>
            await sender.Send(new PlaceKitchenOrderCommand(
                    @event.OrderId,
                    @event.ItemLines)))
                .WithTopic(kitchenOrderedTopic);

        return builder;
    }
}

public record PlaceKitchenOrderCommand(Guid OrderId, List<OrderItemDto> ItemLines) : IRequest<IResult>;

internal class PlaceKitchenOrderCommandValidator : AbstractValidator<PlaceKitchenOrderCommand>
{
}

public class PlaceKitchenOrderCommandHandler(DaprClient daprClient, ILogger<PlaceKitchenOrderCommandHandler> logger) : IRequestHandler<PlaceKitchenOrderCommand, IResult>
{
    private readonly ILogger<PlaceKitchenOrderCommandHandler> _logger = logger;

    public async Task<IResult> Handle(PlaceKitchenOrderCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogInformation("Order info: {OrderInfo}", JsonConvert.SerializeObject(request));

        var message = new KitchenOrderUpdated
        {
            OrderId = request.OrderId
        };

        foreach (var itemLine in request.ItemLines)
        {
            // var kitchenOrder = KitchenOrder.From(request.OrderId, itemLine.ItemType, DateTime.UtcNow);

            await Task.Delay(CalculateDelay(itemLine.ItemType), cancellationToken);

            // var kitchenItemState = kitchenOrder.SetTimeUp(itemLine.ItemLineId, DateTime.UtcNow);

            // await daprClient.SaveStateAsync("statestore", $"order-{request.OrderId}", kitchenItemState, cancellationToken: cancellationToken);
            await daprClient.SaveStateAsync("statestore", $"order-{request.OrderId}", request, cancellationToken: cancellationToken);

            message.ItemLines.Add(new OrderItemDto(itemLine.ItemLineId, itemLine.ItemType));

            // if (kitchenItemState.DomainEvents is not null)
            // {
            //     var @events = new IDomainEvent[kitchenItemState.DomainEvents.Count];
            //     kitchenItemState.DomainEvents.CopyTo(@events);
            //     kitchenItemState.DomainEvents.Clear();

            //     var message = new KitchenOrderUpdated
            //     {
            //         OrderId = request.OrderId
            //     };
            //     foreach (var @event in @events)
            //     {
            //         if (@event is KitchenOrderUp kitchenOrderUp)
            //         {
            //             message.ItemLines.Add(new OrderItemDto(kitchenOrderUp.ItemLineId, kitchenOrderUp.ItemType));
            //         }
            //     }

            //     await daprClient.PublishEventAsync(
            //                 "kitchenpubsub",
            //                 "kitchenorderupdated",
            //                 message,
            //                 cancellationToken);
            // }
        }

        await daprClient.PublishEventAsync(
                            "kitchenpubsub",
                            "kitchenorderupdated",
                            message,
                            cancellationToken);

        return Results.Ok();
    }

    private static TimeSpan CalculateDelay(ItemType itemType)
    {
        return itemType switch
        {
            ItemType.CROISSANT => TimeSpan.FromSeconds(7),
            ItemType.CROISSANT_CHOCOLATE => TimeSpan.FromSeconds(7),
            ItemType.CAKEPOP => TimeSpan.FromSeconds(5),
            ItemType.MUFFIN => TimeSpan.FromSeconds(7),
            _ => TimeSpan.FromSeconds(3)
        };
    }
}