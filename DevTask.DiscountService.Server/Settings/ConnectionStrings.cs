namespace DevTask.DiscountService.Server.Settings;

public class ConnectionStrings
{
    public required string DiscountDb { get; init; }
    public required string Redis { get; init; }
}