using ProductApi.Domain;

namespace ProductApi.Dtos;

public record ItemDto(ItemType Type, decimal Price);

public class ItemTypeDto
{
    public ItemType ItemType { get; set; }
    public string Name { get; set; } = null!;
}