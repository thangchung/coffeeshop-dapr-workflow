using CounterApi.Domain;
using CounterApi.Domain.DomainEvents;
using CounterApi.Domain.Messages;

using Dapr.Client;
using Dapr.Workflow;

namespace CounterApi.Activities;
public class BaristaUpdateOrderActivity(DaprClient daprClient, ILogger<BaristaUpdateOrderActivity> logger) : WorkflowActivity<BaristaOrderUpdated, object?>
{
    public override async Task<object?> RunAsync(WorkflowActivityContext context, BaristaOrderUpdated input)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(input);

        logger.LogInformation("Order is {OrderId}", input.OrderId);

        var orderState = await daprClient.GetStateEntryAsync<Order>("statestore", $"order-{input.OrderId}");
        if (orderState.Value is not null)
        {
            foreach (var lineItem in input.ItemLines)
            {
                logger.LogInformation("Order is {OrderId}, updated BaristaOrderItem={BaristaOrderItemId}", input.OrderId,
                    lineItem.ItemLineId);
                _ = orderState.Value.Apply(new OrderUp(lineItem.ItemLineId));
            }

            await orderState.SaveAsync();
        }

        return Task.FromResult<object?>(null);
    }
}
