using CounterApi.Domain.Dtos;

namespace CounterApi.Domain.Messages;

public record BaristaOrderPlaced
{
    public Guid OrderId { get; init; }
    public List<OrderItemDto> ItemLines { get; init; } = new();
}

public record KitchenOrderPlaced
{
    public Guid OrderId { get; init; }
    public List<OrderItemDto> ItemLines { get; init; } = new();
}