using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SecureSoftwareGroupProject.Data;
using SecureSoftwareGroupProject.Models;  // CustomerBalance
using SecureSoftwareGroupProject.Pages;   // CustomerModel
using Xunit;

public class CustomerModelTests
{
    private static AppDbContext NewDb(string name)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(name)
            .Options;

        var db = new AppDbContext(options);

        db.CustomerBalances.AddRange(
            NewBalance(1, "Alice", "A001", "Active", "AUD", 150, 1000, "Adelaide", "alice@example.com", "0400000001", 5),
            NewBalance(2, "Bob", "B002", "Inactive", "USD", 50, 500, "Sydney", "bob@example.com", "0400000002", 2),
            NewBalance(3, "Charlie", "C003", "Active", "AUD", 300, 1200, "Brisbane", "charlie@example.com", "0400000003", 10)
        );

        db.SaveChanges();
        return db;
    }

    private static CustomerBalance NewBalance(
        int id, string name, string acct, string status, string currency,
        decimal balance, decimal credit, string city, string email, string phone, int daysAgo)
    {
        return new CustomerBalance
        {
            Id = id,
            CustomerName = name,
            AccountNumber = acct,
            Status = status,
            Currency = currency,
            Balance = balance,
            CreditLimit = credit,
            City = city,
            Email = email,
            PhoneNumber = phone,
            Address = "123 Street",
            State = "SA",
            PostalCode = "5000",
            LastPaymentDate = DateTime.Today.AddDays(-daysAgo),
            RegistrationDate = DateTime.Today.AddDays(-100)
        };
    }

    [Fact]
    public async Task OnGetAsync_Default_ReturnsAllAndBuildsCurrencies()
    {
        using var db = NewDb(nameof(OnGetAsync_Default_ReturnsAllAndBuildsCurrencies));
        var page = new CustomerModel(db);

        await page.OnGetAsync(CancellationToken.None);

        Assert.Equal(3, page.Rows.Count);
        Assert.Equal(new[] { "AUD", "USD" }, page.AllCurrencies);
    }

    [Fact]
    public async Task OnGetAsync_StatusFilter_Works()
    {
        using var db = NewDb(nameof(OnGetAsync_StatusFilter_Works));
        var page = new CustomerModel(db)
        {
            Status = "Active"
        };

        await page.OnGetAsync(CancellationToken.None);

        Assert.All(page.Rows, r => Assert.Equal("Active", r.Status));
        Assert.Equal(2, page.Rows.Count);
    }

    [Fact]
    public async Task OnGetAsync_CurrencyFilter_Works()
    {
        using var db = NewDb(nameof(OnGetAsync_CurrencyFilter_Works));
        var page = new CustomerModel(db)
        {
            Currency = "USD"
        };

        await page.OnGetAsync(CancellationToken.None);

        Assert.Single(page.Rows);
        Assert.Equal("USD", page.Rows[0].Currency);
    }

    [Fact]
    public async Task OnGetAsync_MinBalanceFilter_Works()
    {
        using var db = NewDb(nameof(OnGetAsync_MinBalanceFilter_Works));
        var page = new CustomerModel(db)
        {
            MinBalance = 100m
        };

        await page.OnGetAsync(CancellationToken.None);

        Assert.Equal(2, page.Rows.Count);
        Assert.DoesNotContain(page.Rows, r => r.Balance < 100m);
    }

    [Fact]
    public async Task OnGetAsync_QueryFilter_SearchesNameAndCity()
    {
        using var db = NewDb(nameof(OnGetAsync_QueryFilter_SearchesNameAndCity));

        var page = new CustomerModel(db)
        {
            Query = "alice"
        };

        await page.OnGetAsync(CancellationToken.None);

        Assert.Single(page.Rows);
        Assert.Equal("Alice", page.Rows[0].CustomerName);
    }
}