using System.Collections.Concurrent;
using DevTask.DiscountService.Server.Common;
using DevTask.DiscountService.Server.Data;
using DevTask.DiscountService.Server.Settings;
using DevTask.DiscountService.Server.Utils;
using Microsoft.EntityFrameworkCore;

namespace DevTask.DiscountService.Server.Services;

public class DiscountCodeService(
    DiscountContext db,
    ILogger<DiscountCodeService> logger,
    DiscountSettings discountSettings) : IDiscountCodeService
{
    private static readonly HashSet<uint> AllowedCodeLengths = [7, 8];
    public async Task<ResultCode> Generate(uint numberOfCodes, uint codeLength, CancellationToken cancelToken = default)
    {
        if (numberOfCodes < 1 || numberOfCodes > discountSettings.MaxGenerateCount)
        {
            logger.LogWarning("Invalid number of codes ({numberOfCodes})", numberOfCodes);
            return ResultCode.InvalidArgument;
        }
        if (!AllowedCodeLengths.Contains(codeLength))
        {
            logger.LogError("Code length must be 7 or 8 (received {length})", codeLength);
            return ResultCode.InvalidArgument;
        }
        logger.LogInformation("Generating {numberOfCodes} discount codes with length={codeLength}", numberOfCodes, codeLength);
        try
        {
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = discountSettings.MaxParallelTasks,
                CancellationToken = cancelToken
            };
            var codesToInsert = new ConcurrentBag<DiscountCode>();
            await Parallel.ForAsync<uint>(0, numberOfCodes, parallelOptions, (_, cancel) =>
                {
                    if (!cancel.IsCancellationRequested)
                    {
                        codesToInsert.Add(new DiscountCode
                        {
                            Id = Guid.NewGuid(),
                            Code = CodeGenerator.GenerateRandomString(Convert.ToInt32(codeLength)),
                            CreatedUtc = DateTime.UtcNow
                        });
                    }
                    return ValueTask.CompletedTask;
                }
            );
            if (cancelToken.IsCancellationRequested)
            {
                return ResultCode.Cancelled;
            }
            
            db.DiscountCodes.AddRange(codesToInsert);

            await db.SaveChangesAsync(cancelToken);
            logger.LogInformation("Generated {numberOfCodes} discount codes", numberOfCodes);
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Code generation cancelled");
            return ResultCode.Cancelled;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to generate discount codes");
            return ResultCode.Error;
        }
        return ResultCode.Ok;
    }

    public async Task<ResultCode> UseCode(string code, CancellationToken cancelToken = default)
    {
        try
        {
            DiscountCode? existingCode = await db.DiscountCodes.FirstOrDefaultAsync(c=> c.Code == code, cancelToken);
            if (existingCode is null)
            {
                logger.LogWarning("Code {code} was not found", code);
                return ResultCode.InvalidArgument;
            }
            if (existingCode.Used)
            {
                logger.LogWarning("Code {code} is already used", code);
                return ResultCode.InvalidArgument;
            }
            logger.LogInformation("Using code {code}", code);
            existingCode.Used = true;
            existingCode.UsedUtc = DateTime.UtcNow;
            await db.SaveChangesAsync(cancelToken);
            
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Code usage cancelled");
            return ResultCode.Cancelled;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to use discount codes");
            return ResultCode.Error;
        }
        return ResultCode.Ok;
    }
}