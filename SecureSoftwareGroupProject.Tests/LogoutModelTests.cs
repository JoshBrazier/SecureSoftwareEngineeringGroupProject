using Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Moq;
using SecureSoftwareGroupProject.Pages;


using Microsoft.Extensions.DependencyInjection;

public class LogoutModelTests
{
    [Fact]
    public async Task OnPostAsync_SignsOutAndRedirectsToIndex()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();

        var authMock = new Mock<IAuthenticationService>();
        httpContext.RequestServices = new ServiceCollection()
            .AddSingleton(authMock.Object)
            .BuildServiceProvider();

        var model = new LogoutModel
        {
            PageContext = new PageContext { HttpContext = httpContext }
        };

        // Act
        var result = await model.OnPostAsync();

        // Assert
        authMock.Verify(a =>
            a.SignOutAsync(
                httpContext,
                CookieAuthenticationDefaults.AuthenticationScheme,
                It.IsAny<AuthenticationProperties>()),
            Times.Once);

        var redirect = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Index", redirect.PageName);
    }

    [Fact]
    public async Task OnGetAsync_SignsOutAndRedirectsToIndex()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();

        var authMock = new Mock<IAuthenticationService>();
        httpContext.RequestServices = new ServiceCollection()
            .AddSingleton(authMock.Object)
            .BuildServiceProvider();

        var model = new LogoutModel
        {
            PageContext = new PageContext { HttpContext = httpContext }
        };

        // Act
        var result = await model.OnGetAsync();

        // Assert
        authMock.Verify(a =>
            a.SignOutAsync(
                httpContext,
                CookieAuthenticationDefaults.AuthenticationScheme,
                It.IsAny<AuthenticationProperties>()),
            Times.Once);

        var redirect = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Index", redirect.PageName);
    }
}