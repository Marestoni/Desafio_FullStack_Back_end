using Xunit;

namespace EduGraphScheduler.Tests;

public class SimpleTest
{
    [Fact]
    public void One_Plus_One_Should_Be_Two()
    {
        // Arrange
        var a = 1;
        var b = 1;

        // Act
        var result = a + b;

        // Assert
        Assert.Equal(2, result);
    }
}