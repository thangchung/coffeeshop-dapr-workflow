using CounterApi.Domain;
using CounterApi.Domain.Commands;
using CounterApi.Domain.DomainEvents;
using CounterApi.Domain.Dtos;
using CounterApi.Domain.Messages;
using CounterApi.Domain.SharedKernel;
using CounterApi.Workflows;

using Dapr.Client;
using Dapr.Workflow;

namespace CounterApi.Activities;

public class AddOrderActivity(DaprClient daprClient, IItemGateway itemGateway, ILoggerFactory loggerFactory)
    : WorkflowActivity<PlaceOrderCommand, PlaceOrderResult?>
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<NotifyActivity>();

    public override async Task<PlaceOrderResult?> RunAsync(WorkflowActivityContext context, PlaceOrderCommand input)
    {
        var orderId = context.InstanceId;

        _logger.LogInformation("Run AddOrderActivity with orderId={orderId}", orderId);

        var order = await Order.From(input, itemGateway);
        order.Id = new Guid(orderId); //todo: not good

        await daprClient.SaveStateAsync("statestore", $"order-{order.Id}", order);

        var @events = new IDomainEvent[order.DomainEvents.Count];
        order.DomainEvents.CopyTo(@events);
        order.DomainEvents.Clear();

        var baristaEvents = new Dictionary<Guid, BaristaOrderPlaced>();
        var kitchenEvents = new Dictionary<Guid, KitchenOrderPlaced>();
        foreach (var @event in @events)
        {
            switch (@event)
            {
                case BaristaOrderIn baristaOrderInEvent:
                    if (!baristaEvents.TryGetValue(baristaOrderInEvent.OrderId, out _))
                    {
                        baristaEvents.Add(baristaOrderInEvent.OrderId, new BaristaOrderPlaced
                        {
                            OrderId = baristaOrderInEvent.OrderId,
                            ItemLines = new List<OrderItemDto>
                            {
                                new(baristaOrderInEvent.ItemLineId, baristaOrderInEvent.ItemType)
                            }
                        });
                    }
                    else
                    {
                        baristaEvents[baristaOrderInEvent.OrderId].ItemLines.Add(
                            new OrderItemDto(baristaOrderInEvent.ItemLineId, baristaOrderInEvent.ItemType));
                    }

                    break;
                case KitchenOrderIn kitchenOrderInEvent:
                    if (!kitchenEvents.TryGetValue(kitchenOrderInEvent.OrderId, out _))
                    {
                        kitchenEvents.Add(kitchenOrderInEvent.OrderId, new KitchenOrderPlaced
                        {
                            OrderId = kitchenOrderInEvent.OrderId,
                            ItemLines = new List<OrderItemDto>
                            {
                                new(kitchenOrderInEvent.ItemLineId, kitchenOrderInEvent.ItemType)
                            }
                        });
                    }
                    else
                    {
                        kitchenEvents[kitchenOrderInEvent.OrderId].ItemLines.Add(
                            new OrderItemDto(kitchenOrderInEvent.ItemLineId, kitchenOrderInEvent.ItemType));
                    }

                    break;
            }
        }

        if (baristaEvents.Count > 0)
        {
            foreach (var @event in baristaEvents)
            {
                await daprClient.PublishEventAsync(
                    "baristapubsub",
                    nameof(BaristaOrderPlaced).ToLowerInvariant(),
                    @event.Value);
            }
        }

        if (kitchenEvents.Count > 0)
        {
            foreach (var @event in kitchenEvents)
            {
                await daprClient.PublishEventAsync(
                    "kitchenpubsub",
                    nameof(KitchenOrderPlaced).ToLowerInvariant(),
                    @event.Value);
            }
        }

        return new PlaceOrderResult(true);
    }
}