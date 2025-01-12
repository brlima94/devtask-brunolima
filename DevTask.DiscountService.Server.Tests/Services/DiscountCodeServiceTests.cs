using DevTask.DiscountService.Server.Common;
using DevTask.DiscountService.Server.Data;
using DevTask.DiscountService.Server.Services;
using DevTask.DiscountService.Server.Settings;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace DevTask.DiscountService.Server.Tests.Services;

public class DiscountCodeServiceTests
{
    private readonly Mock<ILogger<DiscountCodeService>> _loggerMock;
    private readonly DiscountSettings _settings;
    private readonly DbContextOptions<DiscountContext> _dbContextOptions;

    public DiscountCodeServiceTests()
    {
        _loggerMock = new Mock<ILogger<DiscountCodeService>>();
        _settings = new DiscountSettings
        {
            ConnectionStrings = new ConnectionStrings
            {
                DiscountDb = "test",
                Redis = "test"
            },
            MaxGenerateCount = 1000,
            MaxParallelTasks = 5
        };
        
        _dbContextOptions = new DbContextOptionsBuilder<DiscountContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Theory]
    [InlineData(0)]  // Invalid count
    [InlineData(1001)] // Exceeds max count
    public async Task Generate_InvalidCount_ReturnsInvalidArgument(uint count)
    {
        // Arrange
        await using var context = new DiscountContext(_dbContextOptions);
        var service = new DiscountCodeService(context, _loggerMock.Object, _settings);

        // Act
        var result = await service.Generate(count, 8);

        // Assert
        result.Should().Be(ResultCode.InvalidArgument);
    }

    [Theory]
    [InlineData(6)]  // Too short
    [InlineData(9)]  // Too long
    public async Task Generate_InvalidLength_ReturnsInvalidArgument(uint length)
    {
        // Arrange
        await using var context = new DiscountContext(_dbContextOptions);
        var service = new DiscountCodeService(context, _loggerMock.Object, _settings);

        // Act
        var result = await service.Generate(10, length);

        // Assert
        result.Should().Be(ResultCode.InvalidArgument);
    }

    [Theory]
    [InlineData(7)]
    [InlineData(8)]
    public async Task Generate_ValidParameters_GeneratesCodesSuccessfully(uint length)
    {
        // Arrange
        const uint count = 10;
        await using var context = new DiscountContext(_dbContextOptions);
        var service = new DiscountCodeService(context, _loggerMock.Object, _settings);

        // Act
        var result = await service.Generate(count, length);

        // Assert
        result.Should().Be(ResultCode.Ok);
        var codes = await context.DiscountCodes.ToListAsync();
        codes.Should().HaveCount((int)count);
        codes.Should().AllSatisfy(code =>
        {
            code.Code.Should().HaveLength((int)length);
            code.Used.Should().BeFalse();
            code.UsedUtc.Should().BeNull();
            code.CreatedUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        });
    }

    [Fact]
    public async Task UseCode_NonExistentCode_ReturnsInvalidArgument()
    {
        // Arrange
        await using var context = new DiscountContext(_dbContextOptions);
        var service = new DiscountCodeService(context, _loggerMock.Object, _settings);

        // Act
        var result = await service.UseCode("NOTFOUND");

        // Assert
        result.Should().Be(ResultCode.InvalidArgument);
    }

    [Fact]
    public async Task UseCode_AlreadyUsedCode_ReturnsInvalidArgument()
    {
        // Arrange
        await using var context = new DiscountContext(_dbContextOptions);
        var code = new DiscountCode
        {
            Id = Guid.NewGuid(),
            Code = "TEST123",
            Used = true,
            CreatedUtc = DateTime.UtcNow.AddHours(-1),
            UsedUtc = DateTime.UtcNow
        };
        await context.DiscountCodes.AddAsync(code);
        await context.SaveChangesAsync();
        
        var service = new DiscountCodeService(context, _loggerMock.Object, _settings);

        // Act
        var result = await service.UseCode("TEST123");

        // Assert
        result.Should().Be(ResultCode.InvalidArgument);
    }

    [Fact]
    public async Task UseCode_ValidCode_MarksAsUsedSuccessfully()
    {
        // Arrange
        const string testCode = "TEST123";
        await using var context = new DiscountContext(_dbContextOptions);
        var code = new DiscountCode
        {
            Id = Guid.NewGuid(),
            Code = testCode,
            CreatedUtc = DateTime.UtcNow.AddHours(-1),
            Used = false
        };
        await context.DiscountCodes.AddAsync(code);
        await context.SaveChangesAsync();
        
        var service = new DiscountCodeService(context, _loggerMock.Object, _settings);

        // Act
        var result = await service.UseCode(testCode);

        // Assert
        result.Should().Be(ResultCode.Ok);
        var usedCode = await context.DiscountCodes.FirstAsync(c => c.Code == testCode);
        usedCode.Used.Should().BeTrue();
        usedCode.UsedUtc.Should().NotBeNull();
        usedCode.UsedUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
} 