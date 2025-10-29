using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SecureSoftwareGroupProject.Data;
using SecureSoftwareGroupProject.Pages;
using SecureSoftwareGroupProject.Models;
using Xunit;

public class ClientsModelTests
{
    private static AppDbContext NewDb(string name)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(name)
            .Options;

        var db = new AppDbContext(options);

        db.CustomerBalances.AddRange(
            new CustomerBalance
            {
                Id = 1,
                AccountNumber = "A123",
                CustomerName = "Alice",
                Status = "Active",
                Email = "alice@example.com",
                City = "Adelaide",
                State = "SA",
                PhoneNumber = "0412345678",
                Balance = 100m,
                CreditLimit = 500m,
                LastPaymentDate = DateTime.Today.AddDays(-10),
                RegistrationDate = DateTime.Today.AddMonths(-2)
            },
            new CustomerBalance
            {
                Id = 2,
                AccountNumber = "B456",
                CustomerName = "Bob",
                Status = "Inactive",
                Email = "bob@example.com",
                City = "Sydney",
                State = "NSW",
                PhoneNumber = "0499999999",
                Balance = 200m,
                CreditLimit = 1000m,
                LastPaymentDate = DateTime.Today.AddDays(-5),
                RegistrationDate = DateTime.Today.AddMonths(-1)
            }
        );
        db.SaveChanges();
        return db;
    }

    [Fact]
    public async Task OnGetAsync_Default_ReturnsAllClients()
    {
        using var db = NewDb(nameof(OnGetAsync_Default_ReturnsAllClients));
        var model = new ClientsModel(db);

        await model.OnGetAsync(CancellationToken.None);

        Assert.NotEmpty(model.Clients);
        Assert.Equal(2, model.Clients.Count);
        Assert.Contains(model.Clients, c => c.Name == "Alice");
        Assert.Contains(model.Clients, c => c.Name == "Bob");
        Assert.NotEmpty(model.StatusOptions);
    }

    [Fact]
    public async Task OnGetAsync_StatusFilter_Works()
    {
        using var db = NewDb(nameof(OnGetAsync_StatusFilter_Works));
        var model = new ClientsModel(db)
        {
            Status = "Active"
        };

        await model.OnGetAsync(CancellationToken.None);

        Assert.Single(model.Clients);
        Assert.Equal("Alice", model.Clients.First().Name);
    }

    [Fact]
    public async Task OnGetAsync_SearchFilter_Works()
    {
        using var db = NewDb(nameof(OnGetAsync_SearchFilter_Works));
        var model = new ClientsModel(db)
        {
            Search = "bob"
        };

        await model.OnGetAsync(CancellationToken.None);

        Assert.Single(model.Clients);
        Assert.Equal("Bob", model.Clients.First().Name);
    }
}