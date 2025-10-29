using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using BCrypt.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using SecureSoftwareGroupProject.Data;
using SecureSoftwareGroupProject.Models;
using SecureSoftwareGroupProject.Pages;
using Xunit;
using Microsoft.Extensions.DependencyInjection;

using PageResult = Microsoft.AspNetCore.Mvc.RazorPages.PageResult;
public class LoginModelTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new AppDbContext(options);
        return db;
    }

    private static DefaultHttpContext NewHttpContext()
    {
        var http = new DefaultHttpContext();
        var authService = new Mock<IAuthenticationService>();
        authService
            .Setup(a => a.SignInAsync(
                http,
                CookieAuthenticationDefaults.AuthenticationScheme,
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<AuthenticationProperties>()))
            .Returns(Task.CompletedTask);
        http.RequestServices = new ServiceCollection()
            .AddSingleton(authService.Object)
            .BuildServiceProvider();
        return http;
    }

    [Fact]
    public async Task OnPostAsync_SuccessfulLogin_RedirectsToClients()
    {
        // Arrange
        using var db = CreateDb();
        var password = "Password123!";
        db.Users.Add(new User
        {
            Username = "testuser",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = "User"
        });
        db.SaveChanges();

        var model = new LoginModel(db)
        {
            Username = "testuser",
            Password = password,
            PageContext = new Microsoft.AspNetCore.Mvc.RazorPages.PageContext
            {
                HttpContext = NewHttpContext()
            }
        };

        // Act
        var result = await model.OnPostAsync();

        // Assert
        var redirect = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Clients", redirect.PageName);
    }

    [Fact]
    public async Task OnPostAsync_InvalidPassword_ReturnsPageWithError()
    {
        using var db = CreateDb();
        db.Users.Add(new User
        {
            Username = "user1",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctpass")
        });
        db.SaveChanges();

        var model = new LoginModel(db)
        {
            Username = "user1",
            Password = "wrongpass",
            PageContext = new Microsoft.AspNetCore.Mvc.RazorPages.PageContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        var result = await model.OnPostAsync();

        var page = Assert.IsType<PageResult>(result);
        Assert.Equal("Invalid username or password.", model.ErrorMessage);
    }

    [Fact]
    public async Task OnPostAsync_UserDoesNotExist_ReturnsPageWithError()
    {
        using var db = CreateDb();
        var model = new LoginModel(db)
        {
            Username = "nouser",
            Password = "whatever",
            PageContext = new Microsoft.AspNetCore.Mvc.RazorPages.PageContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        var result = await model.OnPostAsync();

        Assert.IsType<PageResult>(result);
        Assert.Equal("Invalid username or password.", model.ErrorMessage);
    }
}