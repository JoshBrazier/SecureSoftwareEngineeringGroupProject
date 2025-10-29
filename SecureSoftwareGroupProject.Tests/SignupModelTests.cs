using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BCrypt.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SecureSoftwareGroupProject.Data;
using SecureSoftwareGroupProject.Models;
using SecureSoftwareGroupProject.Pages;
using Xunit;

namespace SecureSoftwareGroupProject.Tests.Pages
{
    public class SignupModelTests
    {
        // ---------- helpers ----------
        private static AppDbContext NewDb(string name)
        {
            var opts = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"SignupTests_{name}_{Guid.NewGuid():N}")
                .Options;
            return new AppDbContext(opts);
        }

        private static (DefaultHttpContext http, IServiceProvider sp) NewHttpWithAuth()
        {
            var services = new ServiceCollection();

            // Minimal auth so HttpContext.SignInAsync works
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, _ => { });

            // You typically don’t need authorization here, but harmless if added
            services.AddAuthorization();

            var sp = services.BuildServiceProvider();

            var http = new DefaultHttpContext
            {
                RequestServices = sp
            };

            // establish a principal (not strictly required before SignIn)
            http.User = new ClaimsPrincipal(new ClaimsIdentity());

            return (http, sp);
        }

        private static SignupModel NewModel(AppDbContext db, DefaultHttpContext http)
        {
            var model = new SignupModel(db)
            {
                PageContext = new PageContext
                {
                    HttpContext = http
                }
            };
            return model;
        }

        // ---------- tests ----------

        [Fact]
        public async Task OnPostAsync_InvalidModel_ReturnsPage()
        {
            // Arrange
            using var db = NewDb(nameof(OnPostAsync_InvalidModel_ReturnsPage));
            var (http, _) = NewHttpWithAuth();
            var model = NewModel(db, http);

            model.Username = "ab"; // too short per [StringLength(MinimumLength=3)]
            model.Password = "Bad#1"; // too short and likely fails regex
            model.ConfirmPassword = "Mismatch";
            // Make ModelState invalid (since automatic validation isn’t invoked in unit tests)
            model.ModelState.AddModelError("Username", "too short");
            model.ModelState.AddModelError("Password", "invalid");
            model.ModelState.AddModelError("ConfirmPassword", "mismatch");

            // Act
            var result = await model.OnPostAsync();

            // Assert
            Assert.IsType<PageResult>(result);
            Assert.True(model.ModelState.ErrorCount >= 1);
            Assert.Equal(0, db.Users.Count());
        }

        [Fact]
        public async Task OnPostAsync_UsernameTaken_ReturnsPageWithError()
        {
            // Arrange
            using var db = NewDb(nameof(OnPostAsync_UsernameTaken_ReturnsPageWithError));
            var (http, _) = NewHttpWithAuth();
            var model = NewModel(db, http);

            // existing user
            db.Users.Add(new User
            {
                // If your User.Id is int, no need to set; EF InMemory will handle key assignment if configured.
                Username = "existinguser",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Some!Passw0rd"),
                Password = "placeholder" // stored per your model’s NOT NULL
            });
            await db.SaveChangesAsync();

            model.Username = "existinguser";
            model.Password = "Valid!Passw0rd1";
            model.ConfirmPassword = "Valid!Passw0rd1";

            // Act
            var result = await model.OnPostAsync();

            // Assert
            var page = Assert.IsType<PageResult>(result);
            Assert.True(model.ModelState.ContainsKey(nameof(model.Username)));
            Assert.Equal(1, db.Users.Count()); // nothing added
        }

        [Fact]
        public async Task OnPostAsync_ValidInput_CreatesUserAndSignsIn_RedirectsToCustomer()
        {
            // Arrange
            using var db = NewDb(nameof(OnPostAsync_ValidInput_CreatesUserAndSignsIn_RedirectsToCustomer));
            var (http, sp) = NewHttpWithAuth();
            var model = NewModel(db, http);

            model.Username = "newuser";
            model.Password = "Strong!Passw0rd";
            model.ConfirmPassword = "Strong!Passw0rd";

            // Act
            var result = await model.OnPostAsync();

            // Assert: redirect
            var redirect = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("/Customer", redirect.PageName);

            // Assert: user created with hashed password
            var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Username == "newuser");
            Assert.NotNull(user);
            Assert.False(string.IsNullOrWhiteSpace(user!.PasswordHash));
            Assert.True(BCrypt.Net.BCrypt.Verify("Strong!Passw0rd", user.PasswordHash));

            // Assert: cookie auth sign-in happened
            // (A lightweight check: after SignInAsync, the auth service will have set a cookie header)
            // In unit tests this can be tricky to assert directly; the redirect happening after SignIn is a good proxy.
            // If you want to be strict, you can inspect http.Response.Headers["Set-Cookie"].
            Assert.Contains("Set-Cookie", http.Response.Headers.Keys);
        }
    }
}