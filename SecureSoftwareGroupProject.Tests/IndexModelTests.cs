using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using SecureSoftwareGroupProject.Data;
using SecureSoftwareGroupProject.Pages;
using SecureSoftwareGroupProject.Models;
using SecureSoftwareGroupProject.Model;

public class IndexModelTests
{
    private static AppDbContext NewDb(string name)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(name)
            .Options;
        var db = new AppDbContext(options);
        return db;
    }

    [Fact]
    public async Task OnGetAsync_LoadsTopProvidersAndReviews()
    {
        // Arrange
        using var db = NewDb(nameof(OnGetAsync_LoadsTopProvidersAndReviews));

        // Seed providers
        db.ProviderProfiles.AddRange(
            new ProviderProfile
            {
                Id = Guid.NewGuid(),
                ProfessionalTitle = "Electrician",
                BusinessName = "Bright Sparks",
                RatingAverage = 4.8m,
                RatingCount = 20,
                CompletedJobsCount = 50,
                HourlyRateAmount = 90,
                CalloutFeeAmount = 50,
                ServiceCategoriesCsv = "Electrical",
                EmergencyAvailableFlag = true
            },
            new ProviderProfile
            {
                Id = Guid.NewGuid(),
                ProfessionalTitle = "Plumber",
                BusinessName = "FlowPro",
                RatingAverage = 4.3m,
                RatingCount = 15,
                CompletedJobsCount = 30,
                HourlyRateAmount = 80,
                CalloutFeeAmount = 40,
                ServiceCategoriesCsv = "Plumbing",
                EmergencyAvailableFlag = false
            });

        // Seed reviews
        db.CustomerReviews.AddRange(
            new CustomerReview
            {
                Id = Guid.NewGuid(),
                CustomerName = "Alice",
                Rating = 5,
                ReviewText = "Fantastic service!",
                CreatedAtUtc = DateTime.UtcNow.AddDays(-1)
            },
            new CustomerReview
            {
                Id = Guid.NewGuid(),
                CustomerName = "Bob",
                Rating = 4,
                ReviewText = "Pretty good!",
                CreatedAtUtc = DateTime.UtcNow.AddDays(-2)
            });

        await db.SaveChangesAsync();

        var model = new IndexModel(db);

        // Act
        await model.OnGetAsync(CancellationToken.None);

        // Assert
        Assert.NotEmpty(model.TopProviders);
        Assert.NotEmpty(model.TopReviews);
        Assert.Equal(2, model.TopProviders.Count);
        Assert.Equal(2, model.TopReviews.Count);

        var topProvider = model.TopProviders.First();
        Assert.Equal("Electrician", topProvider.Title);
        Assert.Equal("Bright Sparks", topProvider.BusinessName);
        Assert.True(topProvider.RatingAverage >= 4.3m);

        var review = model.TopReviews.First();
        Assert.Equal("Alice", review.CustomerName);
        Assert.Contains("service", review.ReviewText, StringComparison.OrdinalIgnoreCase);
    }
}