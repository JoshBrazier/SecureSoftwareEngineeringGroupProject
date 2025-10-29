using Xunit;
using SecureSoftwareGroupProject.Pages;

public class ErrorModelTests
{
    [Fact]
    public void ShowRequestId_ReturnsTrue_WhenRequestIdIsNotEmpty()
    {
        // Arrange
        var model = new ErrorModel { RequestId = "12345" };

        // Act
        var result = model.ShowRequestId;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ShowRequestId_ReturnsFalse_WhenRequestIdIsNullOrEmpty()
    {
        // Arrange
        var model = new ErrorModel { RequestId = null };

        // Act
        var result = model.ShowRequestId;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void OnGet_DoesNotThrow()
    {
        // Arrange
        var model = new ErrorModel();

        // Act & Assert
        var ex = Record.Exception(() => model.OnGet());
        Assert.Null(ex);
    }
}