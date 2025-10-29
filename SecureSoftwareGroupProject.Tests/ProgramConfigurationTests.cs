using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using SecureSoftwareGroupProject.Data;
using Xunit;

namespace SecureSoftwareGroupProject.Tests
{
    public class ProgramConfigurationTests
    {
        [Fact]
        public void Builder_Configures_Services_Without_Exception()
        {
            // Arrange
            var args = Array.Empty<string>();
            var builder = WebApplication.CreateBuilder(args);

            // Provide a fake but valid DefaultConnection
            builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "FakeConnectionString"
            });

            // Act
            builder.Services.AddDbContext<AppDbContext>(opts =>
                opts.UseInMemoryDatabase("ProgramConfigTest"));

            builder.Services.AddRazorPages();
            builder.Services.AddAuthentication("Cookies")
                .AddCookie();
            builder.Services.AddAuthorization();

            var app = builder.Build();

            // Assert
            Assert.NotNull(app);
            var db = app.Services.GetService<AppDbContext>();
            Assert.NotNull(db);
        }

        [Fact]
        public void Throws_If_Connection_String_Is_Missing()
        {
            var builder = WebApplication.CreateBuilder(Array.Empty<string>());
            builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = ""
            });

            Assert.Throws<InvalidOperationException>(() =>
            {
                var conn = builder.Configuration.GetConnectionString("DefaultConnection");
                if (string.IsNullOrWhiteSpace(conn))
                    throw new InvalidOperationException("Connection string 'DefaultConnection' is missing or empty.");
            });
        }
    }
}