using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SecureSoftwareGroupProject.Model;
using Xunit;

namespace SecureSoftwareGroupProject.Tests.Model
{
    public class SignupDtoTests
    {
        private static IList<ValidationResult> Validate(SignupDto dto, bool validateAll = true)
        {
            var ctx = new ValidationContext(dto);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(dto, ctx, results, validateAll);
            return results;
        }

        [Fact]
        public void ValidModel_PassesValidation()
        {
            var dto = new SignupDto
            {
                Username = "josh_b",
                Password = "StrongPass1!",
                ConfirmPassword = "StrongPass1!"
            };

            var results = Validate(dto);

            Assert.Empty(results);
        }

        [Fact]
        public void Username_TooShort_FailsMinLength()
        {
            var dto = new SignupDto
            {
                Username = "ab", // < 3
                Password = "StrongPass1!",
                ConfirmPassword = "StrongPass1!"
            };

            var results = Validate(dto);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(SignupDto.Username)));
        }

        [Fact]
        public void Password_TooShort_FailsMinLength()
        {
            var dto = new SignupDto
            {
                Username = "validname",
                Password = "12345", // < 6
                ConfirmPassword = "12345"
            };

            var results = Validate(dto);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(SignupDto.Password)));
        }

        [Fact]
        public void ConfirmPassword_Mismatch_FailsCompare()
        {
            var dto = new SignupDto
            {
                Username = "validname",
                Password = "StrongPass1!",
                ConfirmPassword = "Different1!"
            };

            var results = Validate(dto);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(SignupDto.ConfirmPassword)));
        }

        [Fact]
        public void MissingRequiredFields_FailRequired()
        {
            var dto = new SignupDto
            {
                Username = "", // required
                Password = "", // required
                ConfirmPassword = "" // required
            };

            var results = Validate(dto);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(SignupDto.Username)));
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(SignupDto.Password)));
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(SignupDto.ConfirmPassword)));
        }
    }
}