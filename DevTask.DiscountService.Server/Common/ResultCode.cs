namespace DevTask.DiscountService.Server.Common;

/// <summary>
/// RPC result codes
/// </summary>
public enum ResultCode
{
    // Including only the codes used, for a comprehensive list check https://grpc.io/docs/guides/status-codes/
    Ok = 0,
    Cancelled = 1,
    InvalidArgument = 3,
    Error = 13
}