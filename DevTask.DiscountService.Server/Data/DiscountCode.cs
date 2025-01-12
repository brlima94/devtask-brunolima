using System.ComponentModel.DataAnnotations;

namespace DevTask.DiscountService.Server.Data;

public class DiscountCode
{
    public required Guid Id { get; init; }
    [StringLength(maximumLength: 8)]
    public required string Code { get; init; }
    public bool Used { get; set; }
    public required DateTime CreatedUtc { get; init; }
    public DateTime? UsedUtc { get; set; }
}