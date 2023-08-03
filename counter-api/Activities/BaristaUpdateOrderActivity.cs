using CounterApi.Domain;
using CounterApi.Domain.DomainEvents;
using CounterApi.Domain.Dtos;
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
        // var orderState = await daprClient.GetStateEntryAsync<Order>("statestore", $"order-{input.OrderId}");
        var orderState = await daprClient.GetStateEntryAsync<OrderDto>("statestore", $"order-{input.OrderId}");
        logger.LogInformation("orderState: {orderState}", orderState);

        orderState.Value.OrderStatus = OrderStatus.FULFILLED;
        var result = await orderState.TrySaveAsync();
        logger.LogInformation("Order updated = {IsSucceed}", result);

        // todo: refactor 
        
        // if (orderState.Value is not null)
        // {
        //     foreach (var lineItem in input.ItemLines)
        //     {
        //         logger.LogInformation("Order is {OrderId}, updated BaristaOrderItem={BaristaOrderItemId}", input.OrderId,
        //             lineItem.ItemLineId);

        //         orderState.Value = orderState.Value.Apply(new OrderUp(lineItem.ItemLineId));

        //         logger.LogInformation("Order updating is {Order}", orderState.Value);
        //     }

        //     var result = await orderState.TrySaveAsync();
        //     logger.LogInformation("Order updated = {IsSucceed}", result);
        // }

        return Task.FromResult<object?>(null);
    }
}
