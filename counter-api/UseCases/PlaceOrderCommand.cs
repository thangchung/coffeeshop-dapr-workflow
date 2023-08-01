using MediatR;
using FluentValidation;

namespace CounterApi.UseCases;

public enum ItemType
{
    // Beverages
    CAPPUCCINO,
    COFFEE_BLACK,
    COFFEE_WITH_ROOM,
    ESPRESSO,
    ESPRESSO_DOUBLE,
    LATTE,
    // Food
    CAKEPOP,
    CROISSANT,
    MUFFIN,
    CROISSANT_CHOCOLATE
}

public class CommandItem
{
    public ItemType ItemType { get; set; }
}

public enum Location
{
    ATLANTA,
    CHARLOTTE,
    RALEIGH
}

public enum OrderSource
{
    COUNTER,
    WEB
}

public enum CommandType
{
    PLACE_ORDER
}

public enum OrderStatus
{
    PLACED,
    IN_PROGRESS,
    FULFILLED
}

public class PlaceOrderCommand : IRequest<IResult>
{
    public CommandType CommandType { get; set; } = CommandType.PLACE_ORDER;
    public OrderSource OrderSource { get; set; }
    public Location Location { get; set; }
    public Guid LoyaltyMemberId { get; set; }
    public List<CommandItem> BaristaItems { get; set; } = new();
    public List<CommandItem> KitchenItems { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public static class OrderInRouteMapper
{
    public static IEndpointRouteBuilder MapOrderInApiRoutes(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("/v1/api/orders", async (PlaceOrderCommand command, ISender sender) => await sender.Send(command));
        return builder;
    }
}

internal class OrderInValidator : AbstractValidator<PlaceOrderCommand>
{
}

internal class PlaceOrderHandler : IRequestHandler<PlaceOrderCommand, IResult>
{
    private readonly IPublisher _publisher;

    public PlaceOrderHandler(IPublisher publisher)
    {
        _publisher = publisher;
    }

    public async Task<IResult> Handle(PlaceOrderCommand placeOrderCommand, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(placeOrderCommand);

        return Results.Ok();
    }
}
