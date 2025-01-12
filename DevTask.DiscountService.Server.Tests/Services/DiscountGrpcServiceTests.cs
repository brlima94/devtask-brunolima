using DevTask.DiscountService.Server.Common;
using DevTask.DiscountService.Server.Services;
using FluentAssertions;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Moq;
using StackExchange.Redis;

namespace DevTask.DiscountService.Server.Tests.Services;

public class DiscountGrpcServiceTests
{
    private readonly Mock<ILogger<DiscountGrpcService>> _loggerMock;
    private readonly Mock<IDiscountCodeService> _discountCodeServiceMock;
    private readonly Mock<IConnectionMultiplexer> _redisMock;
    private readonly Mock<IDatabase> _databaseMock;
    private readonly ServerCallContext _context;

    public DiscountGrpcServiceTests()
    {
        _loggerMock = new Mock<ILogger<DiscountGrpcService>>();
        _discountCodeServiceMock = new Mock<IDiscountCodeService>();
        _redisMock = new Mock<IConnectionMultiplexer>();
        _databaseMock = new Mock<IDatabase>();
        _redisMock.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(_databaseMock.Object);
        
        // Create a mock ServerCallContext
        var metadata = new Metadata();
        var mockServerCallContext = new Mock<ServerCallContext>();
        _context = mockServerCallContext.Object;
    }

    [Fact]
    public async Task Generate_Success_ReturnsTrue()
    {
        // Arrange
        _discountCodeServiceMock
            .Setup(x => x.Generate(It.IsAny<uint>(), It.IsAny<uint>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultCode.Ok);

        var service = new DiscountGrpcService(_loggerMock.Object, _discountCodeServiceMock.Object, _redisMock.Object);
        var request = new GenerateRequest { Count = 10, Length = 8 };

        // Act
        var response = await service.Generate(request, _context);

        // Assert
        response.Result.Should().BeTrue();
        _discountCodeServiceMock.Verify(
            x => x.Generate(request.Count, request.Length, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Generate_Failure_ReturnsFalse()
    {
        // Arrange
        _discountCodeServiceMock
            .Setup(x => x.Generate(It.IsAny<uint>(), It.IsAny<uint>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultCode.InvalidArgument);

        var service = new DiscountGrpcService(_loggerMock.Object, _discountCodeServiceMock.Object, _redisMock.Object);
        var request = new GenerateRequest { Count = 0, Length = 8 };

        // Act
        var response = await service.Generate(request, _context);

        // Assert
        response.Result.Should().BeFalse();
    }

    [Fact]
    public async Task UseCode_Success_ReturnsOk()
    {
        // Arrange
        const string testCode = "TEST123";
        _databaseMock
            .Setup(x => x.LockTakeAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<TimeSpan>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);
        _discountCodeServiceMock
            .Setup(x => x.UseCode(testCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultCode.Ok);

        var service = new DiscountGrpcService(_loggerMock.Object, _discountCodeServiceMock.Object, _redisMock.Object);
        var request = new UseCodeRequest { Code = testCode };

        // Act
        var response = await service.UseCode(request, _context);

        // Assert
        response.Result.Should().Be((uint)ResultCode.Ok);
        _discountCodeServiceMock.Verify(x => x.UseCode(testCode, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UseCode_FailsToAcquireLock_ReturnsInvalidArgument()
    {
        // Arrange
        const string testCode = "TEST123";
        _databaseMock
            .Setup(x => x.LockTakeAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<TimeSpan>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(false);

        var service = new DiscountGrpcService(_loggerMock.Object, _discountCodeServiceMock.Object, _redisMock.Object);
        var request = new UseCodeRequest { Code = testCode };

        // Act
        var response = await service.UseCode(request, _context);

        // Assert
        response.Result.Should().Be((uint)ResultCode.InvalidArgument);
        _discountCodeServiceMock.Verify(x => x.UseCode(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UseCode_ServiceError_ReturnsError()
    {
        // Arrange
        const string testCode = "TEST123";
        _databaseMock
            .Setup(x => x.LockTakeAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<TimeSpan>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);
        _discountCodeServiceMock
            .Setup(x => x.UseCode(testCode, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test error"));

        var service = new DiscountGrpcService(_loggerMock.Object, _discountCodeServiceMock.Object, _redisMock.Object);
        var request = new UseCodeRequest { Code = testCode };

        // Act
        var response = await service.UseCode(request, _context);

        // Assert
        response.Result.Should().Be((uint)ResultCode.Error);
    }
} 