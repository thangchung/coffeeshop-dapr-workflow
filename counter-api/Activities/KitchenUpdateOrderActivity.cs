using CounterApi.Domain;
using CounterApi.Domain.DomainEvents;
using CounterApi.Domain.Messages;

using Dapr.Client;
using Dapr.Workflow;

namespace CounterApi.Activities;

public class KitchenUpdateOrderActivity(DaprClient daprClient, ILogger<BaristaUpdateOrderActivity> logger) : WorkflowActivity<KitchenOrderUpdated, object?>
{
    public override async Task<object?> RunAsync(WorkflowActivityContext context, KitchenOrderUpdated input)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(input);

        logger.LogInformation("Order is {OrderId}", input.OrderId);

        var orderState = await daprClient.GetStateEntryAsync<Order>("statestore", $"order-{input.OrderId}");
        if (orderState.Value is not null)
        {
            foreach (var lineItem in input.ItemLines)
            {
                logger.LogInformation("Order is {OrderId}, updated KitchenOrderItem={KitchenOrderItemId}", input.OrderId,
                    lineItem.ItemLineId);

                _ = orderState.Value.Apply(new OrderUp(lineItem.ItemLineId));
            }

            await orderState.SaveAsync();
        }

        return Task.FromResult<object?>(null);
    }
}