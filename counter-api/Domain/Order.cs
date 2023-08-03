using System.Text.Json.Serialization;

using CounterApi.Domain.Commands;
using CounterApi.Domain.DomainEvents;
using CounterApi.Domain.SharedKernel;

namespace CounterApi.Domain;

public class Order
{
    [JsonIgnore]
    public HashSet<IDomainEvent> DomainEvents { get; private set; } = new HashSet<IDomainEvent>();
    
    public Guid Id { get; set; } = Guid.NewGuid();
    public OrderSource OrderSource { get; set; }
    public Guid LoyaltyMemberId { get; set; }
    public OrderStatus OrderStatus { get; set; }
    public Location Location { get; set; }
    public List<LineItem> LineItems { get; set; } = new();
    public DateTime Created { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
    public DateTime? Updated { get; set; }

    private Order(OrderSource orderSource, Guid loyaltyMemberId, OrderStatus orderStatus, Location location)
    {
        OrderSource = orderSource;
        LoyaltyMemberId = loyaltyMemberId;
        OrderStatus = orderStatus;
        Location = location;
    }

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

public class LineItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public ItemType ItemType { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public ItemStatus ItemStatus { get; set; }
    public bool IsBaristaOrder { get; set; }

    public LineItem()
    {
    }

    public LineItem(ItemType itemType, string name, decimal price, ItemStatus itemStatus, bool isBarista)
    {
        ItemType = itemType;
        Name = name;
        Price = price;
        ItemStatus = itemStatus;
        IsBaristaOrder = isBarista;
    }
}