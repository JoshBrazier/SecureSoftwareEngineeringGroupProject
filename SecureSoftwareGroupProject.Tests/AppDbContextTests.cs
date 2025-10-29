using Microsoft.EntityFrameworkCore;
using SecureSoftwareGroupProject.Data;
using SecureSoftwareGroupProject.Models;
using SecureSoftwareGroupProject.Model;
using Xunit;
using System.Linq;

namespace SecureSoftwareGroupProject.Tests
{
    public class AppDbContextTests
    {
        private DbContextOptions<AppDbContext> GetInMemoryOptions(string dbName)
        {
            return new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
        }

        [Fact]
        public void CanCreateDatabaseAndAddEntities()
        {
            // Arrange
            var options = GetInMemoryOptions(nameof(CanCreateDatabaseAndAddEntities));
            using var context = new AppDbContext(options);

            var user = new User
            {
                Username = "testuser",
                Password = "password123"
            };

            // <-- FIX: provide required members for CustomerBalance
            var balance = new CustomerBalance
            {
                // required fields (fill with sensible test values)
                AccountNumber = "ACC-0001",       // required
                CustomerName = "Alice Test",      // required
                Status = "Active",                // likely required in the model's logic
                Balance = 50.25m,
                CreditLimit = 200.00m
            };

            // ProviderProfile: include commonly required fields (adjust if your model requires others)
            var provider = new ProviderProfile
            {
                UserId = Guid.NewGuid(),   // ✅ fix: use Guid instead of int
                HourlyRateAmount = 75.50m,
                CalloutFeeAmount = 25.00m
            };

            // CustomerReview: include minimally required fields (adjust to your model if different)
            var review = new CustomerReview
            {
                CustomerName = "Alice Test",
                Rating = 5,
                ReviewText = "Excellent!",
                CreatedAtUtc = System.DateTime.UtcNow,
                UpdatedAtUtc = System.DateTime.UtcNow
            };

            // Act
            context.Users.Add(user);
            context.CustomerBalances.Add(balance);
            context.ProviderProfiles.Add(provider);
            context.CustomerReviews.Add(review);
            context.SaveChanges();

            // Assert - inserted
            Assert.Equal(1, context.Users.Count());
            Assert.Equal(1, context.CustomerBalances.Count());
            Assert.Equal(1, context.ProviderProfiles.Count());
            Assert.Equal(1, context.CustomerReviews.Count());
        }

        [Fact]
        public void UserTableConfiguration_ShouldRequireUsernameAndPassword()
        {
            var options = GetInMemoryOptions(nameof(UserTableConfiguration_ShouldRequireUsernameAndPassword));
            using var context = new AppDbContext(options);

            var entityType = context.Model.FindEntityType(typeof(User));
            Assert.NotNull(entityType);
            Assert.False(entityType!.FindProperty("Username")!.IsNullable);
            Assert.False(entityType.FindProperty("Password")!.IsNullable);
        }

        [Fact]
        public void CustomerBalance_ShouldHaveDecimalPrecision()
        {
            var options = GetInMemoryOptions(nameof(CustomerBalance_ShouldHaveDecimalPrecision));
            using var context = new AppDbContext(options);

            var entityType = context.Model.FindEntityType(typeof(CustomerBalance));
            Assert.NotNull(entityType);
            var balanceProp = entityType!.FindProperty("Balance")!;
            var creditLimitProp = entityType.FindProperty("CreditLimit")!;

            Assert.Equal(18, balanceProp.GetPrecision());
            Assert.Equal(2, balanceProp.GetScale());
            Assert.Equal(18, creditLimitProp.GetPrecision());
            Assert.Equal(2, creditLimitProp.GetScale());
        }

        [Fact]
        public void ProviderProfile_ShouldHavePrecisionOnRates()
        {
            var options = GetInMemoryOptions(nameof(ProviderProfile_ShouldHavePrecisionOnRates));
            using var context = new AppDbContext(options);

            var entityType = context.Model.FindEntityType(typeof(ProviderProfile));
            Assert.NotNull(entityType);
            var hourlyRate = entityType!.FindProperty("HourlyRateAmount")!;
            var calloutFee = entityType.FindProperty("CalloutFeeAmount")!;

            Assert.Equal(18, hourlyRate.GetPrecision());
            Assert.Equal(2, hourlyRate.GetScale());
            Assert.Equal(18, calloutFee.GetPrecision());
            Assert.Equal(2, calloutFee.GetScale());
        }
    }
}