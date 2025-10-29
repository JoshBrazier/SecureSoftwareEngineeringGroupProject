using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureSoftwareGroupProject.Data;
using SecureSoftwareGroupProject.Models;
using SecureSoftwareGroupProject.Pages;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.FileProviders;
using Xunit;


public sealed class CustomerReviewsModelTests : IDisposable
{
    private readonly string _tempRoot;

    public CustomerReviewsModelTests()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "cust-rev-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempRoot);
    }

    public void Dispose()
    {
        try { if (Directory.Exists(_tempRoot)) Directory.Delete(_tempRoot, recursive: true); } catch { /* ignore */ }
    }

    private static AppDbContext NewDb(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        var db = new AppDbContext(options);

        // Seed one Active customer so IsKnownActiveCustomer can pass when needed
        db.CustomerBalances.Add(new CustomerBalance
        {
            AccountNumber = "12345",
            CustomerName = "Alice",
            Status = "Active",
            Balance = 100
        });
        db.SaveChanges();
        return db;
    }

    private IWebHostEnvironment NewEnv() => new FakeEnv(_tempRoot);

    private static FormFile MakeFormFile(byte[] bytes, string fileName)
    {
        var ms = new MemoryStream(bytes);
        return new FormFile(ms, 0, bytes.Length, "ImageUpload", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/octet-stream"
        };
    }

    private static CustomerReviewsModel NewModel(AppDbContext db, IWebHostEnvironment env)
        => new CustomerReviewsModel(db, env);

    // 1) GET load -> NotFound when id missing
    [Fact]
    public async Task OnGetLoad_ReturnsNotFound_WhenMissing()
    {
        using var db = NewDb(nameof(OnGetLoad_ReturnsNotFound_WhenMissing));
        var model = NewModel(db, NewEnv());

        var result = await model.OnGetLoad(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result);
    }

    // 2) GET load -> returns JSON when found
    [Fact]
public async Task OnGetLoad_ReturnsJson_WhenFound()
{
    using var db = NewDb(nameof(OnGetLoad_ReturnsJson_WhenFound));
    var env = NewEnv();
    var model = NewModel(db, env);

    var id = Guid.NewGuid();
    db.CustomerReviews.Add(new CustomerReview
    {
        Id = id,
        CustomerName = "Alice",
        Rating = 4,
        ReviewText = "Nice",
        ImagePath = "/uploads/customer-reviews/a.png",
        CreatedAtUtc = DateTime.UtcNow,
        UpdatedAtUtc = DateTime.UtcNow
    });
    await db.SaveChangesAsync();

    var result = await model.OnGetLoad(id);

    var json = Assert.IsType<JsonResult>(result);
    Assert.NotNull(json.Value);

    // Validate a couple of fields (handle anonymous object shape)
    // Try dynamic first:
    dynamic v = json.Value!;
    Assert.Equal(id, (Guid)v.Id);
    Assert.Equal("Alice", (string)v.CustomerName);
}

    // 3) POST create -> invalid customer name -> Page (ModelState error)
    [Fact]
    public async Task OnPostCreate_InvalidCustomer_ReturnsPage()
    {
        using var db = NewDb(nameof(OnPostCreate_InvalidCustomer_ReturnsPage));
        var model = NewModel(db, NewEnv());
        model.Form = new CustomerReviewForm
        {
            CustomerName = "Unknown Person", // not in Active list
            Rating = 3,
            ReviewText = "meh"
        };

        var result = await model.OnPostCreateAsync();

        var page = Assert.IsType<PageResult>(result);
        Assert.False(model.ModelState.IsValid);
        Assert.True(model.ModelState.ContainsKey("Form.CustomerName"));
        Assert.Empty(db.CustomerReviews); // nothing inserted
    }

    // 4) POST create -> valid (no image) -> Redirect and row created
    [Fact]
    public async Task OnPostCreate_ValidNoImage_CreatesAndRedirects()
    {
        using var db = NewDb(nameof(OnPostCreate_ValidNoImage_CreatesAndRedirects));
        var model = NewModel(db, NewEnv());
        model.Form = new CustomerReviewForm
        {
            CustomerName = "Alice",
            Rating = 4,
            ReviewText = "Solid!"
        };

        var result = await model.OnPostCreateAsync();

        Assert.IsType<RedirectToPageResult>(result);
        var saved = db.CustomerReviews.Single();
        Assert.Equal("Alice", saved.CustomerName);
        Assert.Equal(4, saved.Rating);
        Assert.Equal("Solid!", saved.ReviewText);
    }

    // 5) POST create -> image too big -> Page with error
    [Fact]
    public async Task OnPostCreate_ImageTooBig_ReturnsPageWithError()
    {
        using var db = NewDb(nameof(OnPostCreate_ImageTooBig_ReturnsPageWithError));
        var model = NewModel(db, NewEnv());

        // 3MB payload to exceed 2MB max
        var big = new byte[3 * 1024 * 1024];
        var file = MakeFormFile(big, "big.jpg");

        model.Form = new CustomerReviewForm
        {
            CustomerName = "Alice",
            Rating = 5,
            ReviewText = "big",
            ImageUpload = file
        };

        var result = await model.OnPostCreateAsync();

        Assert.IsType<PageResult>(result);
        Assert.Contains("Form.ImageUpload", model.ModelState.Keys);
        Assert.Empty(db.CustomerReviews);
    }

    // 6) POST update -> missing Id -> Page with error
    [Fact]
    public async Task OnPostUpdate_IdNull_ReturnsPageWithError()
    {
        using var db = NewDb(nameof(OnPostUpdate_IdNull_ReturnsPageWithError));
        var model = NewModel(db, NewEnv());
        model.Form = new CustomerReviewForm
        {
            Id = null,
            CustomerName = "Alice",
            Rating = 1,
            ReviewText = "x"
        };

        var result = await model.OnPostUpdateAsync();

        Assert.IsType<PageResult>(result);
        Assert.False(model.ModelState.IsValid);
    }

    // 7) POST update -> not found -> 404
    [Fact]
    public async Task OnPostUpdate_NotFound_Returns404()
    {
        using var db = NewDb(nameof(OnPostUpdate_NotFound_Returns404));
        var model = NewModel(db, NewEnv());

        // Make ModelState valid
        model.Form = new CustomerReviewForm
        {
            Id = Guid.NewGuid(),
            CustomerName = "Alice",
            Rating = 2,
            ReviewText = "ok"
        };
        // Need active customers loaded inside handler → handled by the method itself.

        var result = await model.OnPostUpdateAsync();

        Assert.IsType<NotFoundResult>(result);
    }

    // 8) POST update -> RemoveImage deletes file and keeps row
    [Fact]
    public async Task OnPostUpdate_RemoveImage_DeletesFile()
    {
        using var db = NewDb(nameof(OnPostUpdate_RemoveImage_DeletesFile));
        var env = NewEnv();

        // Create a dummy file under wwwroot/uploads/customer-reviews/...
        var uploadDir = Path.Combine(env.WebRootPath, "uploads", "customer-reviews");
        Directory.CreateDirectory(uploadDir);
        var fileName = $"{Guid.NewGuid():N}.jpg";
        var fullPath = Path.Combine(uploadDir, fileName);
        await File.WriteAllTextAsync(fullPath, "x");
        var relativePath = $"/uploads/customer-reviews/{fileName}";

        var entity = new CustomerReview
        {
            Id = Guid.NewGuid(),
            CustomerName = "Alice",
            Rating = 5,
            ReviewText = "has image",
            ImagePath = relativePath,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
        db.CustomerReviews.Add(entity);
        await db.SaveChangesAsync();

        var model = NewModel(db, env);
        model.Form = new CustomerReviewForm
        {
            Id = entity.Id,
            CustomerName = "Alice",
            Rating = 4,
            ReviewText = "updated",
            RemoveImage = true
        };

        var result = await model.OnPostUpdateAsync();

        Assert.IsType<RedirectToPageResult>(result);
        var refreshed = await db.CustomerReviews.FindAsync(entity.Id);
        Assert.NotNull(refreshed);
        Assert.Null(refreshed!.ImagePath);
        Assert.False(File.Exists(fullPath)); // file removed
    }

    // 9) POST delete -> not found -> 404
    [Fact]
    public async Task OnPostDelete_NotFound_Returns404()
    {
        using var db = NewDb(nameof(OnPostDelete_NotFound_Returns404));
        var model = NewModel(db, NewEnv());

        var result = await model.OnPostDeleteAsync(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result);
    }

    // 10) POST delete -> removes row (and file if present) and redirects
    [Fact]
    public async Task OnPostDelete_RemovesAndRedirects()
    {
        using var db = NewDb(nameof(OnPostDelete_RemovesAndRedirects));
        var env = NewEnv();

        var uploadDir = Path.Combine(env.WebRootPath, "uploads", "customer-reviews");
        Directory.CreateDirectory(uploadDir);
        var fileName = $"{Guid.NewGuid():N}.png";
        var fullPath = Path.Combine(uploadDir, fileName);
        await File.WriteAllTextAsync(fullPath, "blob");
        var relative = $"/uploads/customer-reviews/{fileName}";

        var entity = new CustomerReview
        {
            Id = Guid.NewGuid(),
            CustomerName = "Alice",
            Rating = 5,
            ReviewText = "del",
            ImagePath = relative,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
        db.CustomerReviews.Add(entity);
        await db.SaveChangesAsync();

        var model = NewModel(db, env);

        var result = await model.OnPostDeleteAsync(entity.Id);

        Assert.IsType<RedirectToPageResult>(result);
        Assert.Empty(db.CustomerReviews);
        Assert.False(File.Exists(fullPath));
    }

    // Lightweight fake IWebHostEnvironment
    private sealed class FakeEnv : IWebHostEnvironment
    {
        public FakeEnv(string root) { WebRootPath = root; ContentRootPath = root; EnvironmentName = "Development"; ApplicationName = "Tests"; WebRootFileProvider = null!; ContentRootFileProvider = null!; }
        public string ApplicationName { get; set; }
        public IFileProvider ContentRootFileProvider { get; set; }
        public string ContentRootPath { get; set; }
        public string EnvironmentName { get; set; }
        public IFileProvider WebRootFileProvider { get; set; }
        public string WebRootPath { get; set; }
    }
}