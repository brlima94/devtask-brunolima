using DevTask.DiscountService.Server.Common;

namespace DevTask.DiscountService.Server.Services;

public interface IDiscountCodeService
{
    Task<ResultCode> Generate(uint numberOfCodes, uint codeLength, CancellationToken cancelToken = default);
    Task<ResultCode> UseCode(string code, CancellationToken cancelToken = default);
}