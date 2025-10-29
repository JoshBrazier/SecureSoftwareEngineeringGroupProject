using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;
using SecureSoftwareGroupProject.Data;
using SecureSoftwareGroupProject.Model;
using SecureSoftwareGroupProject.Pages;

namespace SecureSoftwareGroupProject.Tests
{
    public class PrivacyModelTests
    {
        private static AppDbContext NewDb(string name)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(name)
                .Options;
            return new AppDbContext(options);
        }

        // ---------- OnGet ----------
        [Fact]
        public async Task OnGet_LoadsProviders()
        {
            using var db = NewDb(nameof(OnGet_LoadsProviders));
            db.ProviderProfiles.Add(new ProviderProfile
            {
                Id = Guid.NewGuid(),
                ProfessionalTitle = "Electrician",
                BusinessName = "Bright Sparks",
                CreatedAtUtc = DateTime.UtcNow.AddHours(-1),
                UpdatedAtUtc = DateTime.UtcNow
            });
            await db.SaveChangesAsync();

            var model = new PrivacyModel(db);

            await model.OnGet();

            Assert.Single(model.Rows);
            Assert.Equal("Bright Sparks", model.Rows[0].BusinessName);
        }

        // ---------- OnGetLoad ----------
        [Fact]
        public async Task OnGetLoad_ReturnsJson_WhenProviderExists()
        {
            using var db = NewDb(nameof(OnGetLoad_ReturnsJson_WhenProviderExists));
            var id = Guid.NewGuid();
            db.ProviderProfiles.Add(new ProviderProfile
            {
                Id = id,
                ProfessionalTitle = "Plumber",
                BusinessName = "FlowPro",
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            });
            await db.SaveChangesAsync();

            var model = new PrivacyModel(db);
            var result = await model.OnGetLoad(id);

            var json = Assert.IsType<JsonResult>(result);
            dynamic data = json.Value!;
            Assert.Equal("Plumber", (string)data.ProfessionalTitle);
        }

        [Fact]
        public async Task OnGetLoad_ReturnsNotFound_WhenMissing()
        {
            using var db = NewDb(nameof(OnGetLoad_ReturnsNotFound_WhenMissing));
            var model = new PrivacyModel(db);

            var result = await model.OnGetLoad(Guid.NewGuid());

            Assert.IsType<NotFoundResult>(result);
        }

        // ---------- OnPostCreate ----------
        [Fact]
        public async Task OnPostCreate_CreatesProvider_WhenValid()
        {
            using var db = NewDb(nameof(OnPostCreate_CreatesProvider_WhenValid));
            var model = new PrivacyModel(db)
            {
                Form = new ProviderForm
                {
                    ProfessionalTitle = "Engineer",
                    BusinessName = "TechCorp",
                    YearsExperience = 5,
                    HourlyRateAmount = 120,
                    CalloutFeeAmount = 30,
                    SkillsCsv = "Wiring,Repair",
                    ServiceCategoriesCsv = "Electrical",
                    EmergencyAvailableFlag = true
                }
            };

            var result = await model.OnPostCreate();

            Assert.IsType<RedirectToPageResult>(result);
            Assert.Single(db.ProviderProfiles);
        }

        // ---------- OnPostUpdate ----------
        [Fact]
        public async Task OnPostUpdate_UpdatesProvider_WhenValid()
        {
            using var db = NewDb(nameof(OnPostUpdate_UpdatesProvider_WhenValid));
            var id = Guid.NewGuid();
            db.ProviderProfiles.Add(new ProviderProfile
            {
                Id = id,
                ProfessionalTitle = "Old Title",
                BusinessName = "OldCo",
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            });
            await db.SaveChangesAsync();

            var model = new PrivacyModel(db)
            {
                Form = new ProviderForm
                {
                    Id = id,
                    ProfessionalTitle = "New Title",
                    BusinessName = "NewCo"
                }
            };

            var result = await model.OnPostUpdate();

            Assert.IsType<RedirectToPageResult>(result);
            var updated = await db.ProviderProfiles.FirstAsync();
            Assert.Equal("New Title", updated.ProfessionalTitle);
        }

        // ---------- OnPostDelete ----------
        [Fact]
        public async Task OnPostDelete_RemovesProvider_WhenExists()
        {
            using var db = NewDb(nameof(OnPostDelete_RemovesProvider_WhenExists));
            var id = Guid.NewGuid();
            db.ProviderProfiles.Add(new ProviderProfile
            {
                Id = id,
                ProfessionalTitle = "Cleaner",
                BusinessName = "SparklePro",
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            });
            await db.SaveChangesAsync();

            var model = new PrivacyModel(db);
            var result = await model.OnPostDelete(id);

            Assert.IsType<RedirectToPageResult>(result);
            Assert.Empty(db.ProviderProfiles);
        }
    }
}