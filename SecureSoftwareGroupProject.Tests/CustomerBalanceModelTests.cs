using Microsoft.EntityFrameworkCore;
using SecureSoftwareGroupProject.Data;
using SecureSoftwareGroupProject.Models;
using SecureSoftwareGroupProject.Pages;
using Xunit;

public class CustomerBalanceModelTests
{
    private static AppDbContext NewDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("CustomerBalanceTestDb")
            .Options;

        var db = new AppDbContext(options);

        db.CustomerBalances.AddRange(
            new CustomerBalance
            {
                Id = 1,
                CustomerName = "Alice",
                AccountNumber = "A001",
                Balance = 120.50m,
                CreditLimit = 500m,
                RegistrationDate = DateTime.Today.AddDays(-5)
            },
            new CustomerBalance
            {
                Id = 2,
                CustomerName = "Bob",
                AccountNumber = "B002",
                Balance = 320.00m,
                CreditLimit = 1000m,
                RegistrationDate = DateTime.Today.AddDays(-1)
            }
        );
        db.SaveChanges();
        return db;
    }

    [Fact]
    public async Task OnGetAsync_LoadsBalancesOrderedByRegistrationDateDescending()
    {
        // Arrange
        using var db = NewDb();
        var pageModel = new CustomerBalanceModel(db);

        // Act
        await pageModel.OnGetAsync();

        // Assert
        Assert.NotNull(pageModel.Balances);
        Assert.Equal(2, pageModel.Balances.Count);

        // Check ordering
        Assert.Equal("Bob", pageModel.Balances[0].CustomerName);
        Assert.Equal("Alice", pageModel.Balances[1].CustomerName);

        // Check properties were fetched
        Assert.Equal("B002", pageModel.Balances[0].AccountNumber);
        Assert.Equal(320.00m, pageModel.Balances[0].Balance);
    }
}