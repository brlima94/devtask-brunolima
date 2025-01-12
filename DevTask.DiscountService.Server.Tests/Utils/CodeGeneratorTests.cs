using DevTask.DiscountService.Server.Utils;
using FluentAssertions;

namespace DevTask.DiscountService.Server.Tests.Utils;

public class CodeGeneratorTests
{
    [Theory]
    [InlineData(7)]
    [InlineData(8)]
    public void GenerateRandomString_ReturnsStringWithCorrectLength(int length)
    {
        // Act
        var result = CodeGenerator.GenerateRandomString(length);

        // Assert
        result.Should().HaveLength(length);
    }

    [Theory]
    [InlineData(7)]
    [InlineData(8)]
    public void GenerateRandomString_ReturnsOnlyValidCharacters(int length)
    {
        // Act
        var result = CodeGenerator.GenerateRandomString(length);

        // Assert
        result.Should().MatchRegex("^[A-Za-z0-9]+$");
    }

    [Fact]
    public void GenerateRandomString_GeneratesUniqueStrings()
    {
        // Arrange
        const int count = 1000;
        const int length = 8;
        var generatedCodes = new HashSet<string>();

        // Act
        for (var i = 0; i < count; i++)
        {
            var code = CodeGenerator.GenerateRandomString(  length);
            generatedCodes.Add(code);
        }

        // Assert
        generatedCodes.Should().HaveCount(count, "all generated codes should be unique");
    }
} 