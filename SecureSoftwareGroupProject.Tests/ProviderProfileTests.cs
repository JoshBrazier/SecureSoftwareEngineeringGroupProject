using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using SecureSoftwareGroupProject.Model;
using Xunit;

namespace SecureSoftwareGroupProject.Tests.Models
{
    public class ProviderProfileTests
    {
        [Fact]
        public void DefaultValues_AreInitializedCorrectly()
        {
            // Arrange
            var profile = new ProviderProfile();

            // Assert
            Assert.NotEqual(Guid.Empty, profile.Id);
            Assert.NotNull(profile.CreatedAtUtc);
            Assert.NotNull(profile.UpdatedAtUtc);
            Assert.True(profile.CreatedAtUtc <= DateTime.UtcNow);
            Assert.True(profile.UpdatedAtUtc <= DateTime.UtcNow);
        }

        [Fact]
        public void CanAssign_AllProperties()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var now = DateTime.UtcNow;

            var profile = new ProviderProfile
            {
                Id = id,
                UserId = userId,
                ProfessionalTitle = "Electrician",
                BusinessName = "Bright Sparks",
                YearsExperience = 12,
                HourlyRateAmount = 75.50m,
                CalloutFeeAmount = 30.00m,
                SkillsCsv = "Wiring,Testing",
                CertificationsCsv = "Electrical Safety",
                ServiceCategoriesCsv = "Residential,Commercial",
                ServiceAreaCodesCsv = "5000,5001",
                LanguagesCsv = "English,French",
                EmergencyAvailableFlag = true,
                VehicleType = "Van",
                ToolsOwnedCsv = "Drill,Screwdriver",
                InsuranceProvider = "Allianz",
                InsurancePolicyNumber = "POL12345",
                InsuranceExpiryDateUtc = now.AddYears(1),
                VerificationLevel = "Gold",
                RatingAverage = 4.9m,
                RatingCount = 124,
                CompletedJobsCount = 320,
                ResponseTimeMinutesAvg = 15,
                IntroVideoUrl = "https://example.com/video",
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };

            // Assert
            Assert.Equal(id, profile.Id);
            Assert.Equal(userId, profile.UserId);
            Assert.Equal("Electrician", profile.ProfessionalTitle);
            Assert.Equal("Bright Sparks", profile.BusinessName);
            Assert.Equal(12, profile.YearsExperience);
            Assert.Equal(75.50m, profile.HourlyRateAmount);
            Assert.Equal("Gold", profile.VerificationLevel);
            Assert.Equal(124, profile.RatingCount);
            Assert.True(profile.EmergencyAvailableFlag);
        }

        [Fact]
        public void ValidationAttributes_ArePresent_OnKeyFields()
        {
            // Arrange
            var type = typeof(ProviderProfile);

            // Act
            var titleAttr = type.GetProperty(nameof(ProviderProfile.ProfessionalTitle))!
                .GetCustomAttributes(typeof(RequiredAttribute), false)
                .Cast<RequiredAttribute>()
                .FirstOrDefault();

            var nameAttr = type.GetProperty(nameof(ProviderProfile.ProfessionalTitle))!
                .GetCustomAttributes(typeof(StringLengthAttribute), false)
                .Cast<StringLengthAttribute>()
                .FirstOrDefault();

            // Assert
            Assert.NotNull(titleAttr);
            Assert.NotNull(nameAttr);
            Assert.Equal(120, nameAttr!.MaximumLength);
        }
    }
}