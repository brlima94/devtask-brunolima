using DevTask.DiscountService.Server.Common;
using Grpc.Core;
using StackExchange.Redis;

namespace DevTask.DiscountService.Server.Services;

public class DiscountGrpcService : DiscountService.DiscountServiceBase
{
    private readonly ILogger<DiscountGrpcService> _logger;
    private readonly IDiscountCodeService _discountCodeService;
    private readonly IDatabase _redis;

    public DiscountGrpcService(ILogger<DiscountGrpcService> logger, IDiscountCodeService discountCodeService, IConnectionMultiplexer redis)
    {
        _logger = logger;
        _discountCodeService = discountCodeService;
        _redis = redis.GetDatabase();
    }

    public override async Task<GenerateResponse> Generate(GenerateRequest request, ServerCallContext context)
    {
        ResultCode resultCode = await _discountCodeService.Generate(request.Count, request.Length, context.CancellationToken);
        var response = new GenerateResponse
        {
            Result = resultCode == ResultCode.Ok
        };
        _logger.LogInformation("Generation result={result} ({resultCode})", response.Result, resultCode);
        return response;
    }

    public override async Task<UseCodeResponse> UseCode(UseCodeRequest request, ServerCallContext context)
    {
        ResultCode resultCode;
        string lockKey = $"UseCode:{request.Code}";
        string lockValue = Guid.NewGuid().ToString();
        try
        {
            if (!await _redis.LockTakeAsync(lockKey, lockValue, TimeSpan.FromMinutes(1)))
            {
                _logger.LogWarning("This code is already being used by another request: {code}", request.Code);
                resultCode = ResultCode.InvalidArgument;
            }
            else
            {
                resultCode = await _discountCodeService.UseCode(request.Code, context.CancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to use code {code}", request.Code);
            resultCode = ResultCode.Error;
        }
        finally
        {
            await _redis.LockReleaseAsync(lockKey, lockValue);
        }
        _logger.LogInformation("Use code result={resultCode}", resultCode);
        return new UseCodeResponse
        {
            Result = (uint)resultCode
        };
    }
}