namespace CounterApi.Domain.Dtos;

public record ItemDto(string Name, decimal Price, ItemType ItemType, string Image);

public record ItemTypeDto(ItemType Type, string Name);

public record OrderItemDto(Guid ItemLineId, ItemType ItemType);

public class OrderDto
{
    public Guid Id { get; set; }
    public OrderStatus OrderStatus { get; set; }
}
