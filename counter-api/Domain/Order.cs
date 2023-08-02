using System.Text.Json.Serialization;

using CounterApi.Domain.Commands;
using CounterApi.Domain.DomainEvents;
using CounterApi.Domain.SharedKernel;

namespace CounterApi.Domain;

public class Order(OrderSource orderSource, Guid loyaltyMemberId, OrderStatus orderStatus, Location location)
{
    [JsonIgnore]
    public HashSet<IDomainEvent> DomainEvents { get; private set; } = new HashSet<IDomainEvent>();
    public Guid Id { get; set; } = Guid.NewGuid();
    public OrderSource OrderSource => orderSource;
    public Guid LoyaltyMemberId => loyaltyMemberId;
    public OrderStatus OrderStatus { get; set; } = orderStatus;
    public Location Location => location;
    public List<LineItem> LineItems { get; } = new();
    public DateTime Created { get; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
    public DateTime? Updated { get; }

    public void AddDomainEvent(IDomainEvent eventItem)
    {
        DomainEvents ??= new HashSet<IDomainEvent>();
        DomainEvents.Add(eventItem);
    }

    public void RemoveDomainEvent(IDomainEvent eventItem)
    {
        DomainEvents?.Remove(eventItem);
    }

    public static async Task<Order> From(PlaceOrderCommand placeOrderCommand, IItemGateway itemGateway)
    {
        var order = new Order(placeOrderCommand.OrderSource, placeOrderCommand.LoyaltyMemberId, OrderStatus.IN_PROGRESS, placeOrderCommand.Location)
        {
            Id = placeOrderCommand.OrderId
        };

        if (placeOrderCommand.BaristaItems.Count != 0)
        {
            var itemTypes = placeOrderCommand.BaristaItems.Select(x => x.ItemType);
            var items = await itemGateway.GetItemsByType(itemTypes.ToArray());
            foreach (var baristaItem in placeOrderCommand.BaristaItems)
            {
                var item = items.FirstOrDefault(x => x.ItemType == baristaItem.ItemType);
                var lineItem = new LineItem(baristaItem.ItemType, item?.ItemType.ToString()!, (decimal)item?.Price!, ItemStatus.IN_PROGRESS, true);

                order.AddDomainEvent(new OrderUpdate(order.Id, lineItem.Id, lineItem.ItemType, OrderStatus.IN_PROGRESS));
                order.AddDomainEvent(new BaristaOrderIn(order.Id, lineItem.Id, lineItem.ItemType));

                order.LineItems.Add(lineItem);
            }
        }

        if (placeOrderCommand.KitchenItems.Count != 0)
        {
            var itemTypes = placeOrderCommand.KitchenItems.Select(x => x.ItemType);
            var items = await itemGateway.GetItemsByType(itemTypes.ToArray());
            foreach (var kitchenItem in placeOrderCommand.KitchenItems)
            {
                var item = items.FirstOrDefault(x => x.ItemType == kitchenItem.ItemType);
                var lineItem = new LineItem(kitchenItem.ItemType, item?.ItemType.ToString()!, (decimal)item?.Price!, ItemStatus.IN_PROGRESS, false);

                order.AddDomainEvent(new OrderUpdate(order.Id, lineItem.Id, lineItem.ItemType, OrderStatus.IN_PROGRESS));
                order.AddDomainEvent(new KitchenOrderIn(order.Id, lineItem.Id, lineItem.ItemType));

                order.LineItems.Add(lineItem);
            }
        }

        return order;
    }

    public Order Apply(OrderUp orderUp)
    {
        if (LineItems.Count == 0) return this;

        var item = LineItems.FirstOrDefault(i => i.Id == orderUp.ItemLineId);
        if (item is not null)
        {
            item.ItemStatus = ItemStatus.FULFILLED;
            // AddDomainEvent(new OrderUpdate(Id, item.Id, item.ItemType, OrderStatus.FULFILLED, orderUp.MadeBy));
        }

        // if there are both barista and kitchen items is fulfilled then checking status and change order to Fulfilled
        if (LineItems.All(i => i.ItemStatus == ItemStatus.FULFILLED))
        {
            OrderStatus = OrderStatus.FULFILLED;
        }
        return this;
    }
}

public class LineItem(ItemType itemType, string name, decimal price, ItemStatus itemStatus, bool isBarista)
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public ItemType ItemType => itemType;
    public string Name => name;
    public decimal Price => price;
    public ItemStatus ItemStatus { get; set; } = itemStatus;
    public bool IsBaristaOrder => isBarista;
}