using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SecureSoftwareGroupProject.Models;
using Xunit;

namespace SecureSoftwareGroupProject.Tests.Models
{
    public class UserTests
    {
        private static IList<ValidationResult> Validate(User user)
        {
            var ctx = new ValidationContext(user);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(user, ctx, results, true);
            return results;
        }

        [Fact]
        public void ValidUser_PassesValidation()
        {
            var user = new User
            {
                Id = 1,
                Username = "Josh",
                Password = "placeholder",
                PasswordHash = "hashedpass",
                Role = "User"
            };

            var results = Validate(user);

            Assert.Empty(results);
        }

        [Fact]
        public void MissingUsername_FailsRequiredValidation()
        {
            var user = new User
            {
                Id = 1,
                Password = "plaintext",
                PasswordHash = "hash123",
                Role = "Admin"
            };

            var results = Validate(user);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(User.Username)));
        }

        [Fact]
        public void Username_TooLong_FailsMaxLength()
        {
            var user = new User
            {
                Id = 1,
                Username = new string('A', 65), // exceeds [StringLength(64)]
                Password = "plaintext",
                PasswordHash = "hash123",
                Role = "User"
            };

            var results = Validate(user);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(User.Username)));
        }

        [Fact]
        public void PasswordHash_TooLong_FailsMaxLength()
        {
            var user = new User
            {
                Id = 1,
                Username = "Josh",
                PasswordHash = new string('X', 201), // exceeds [StringLength(200)]
                Role = "User"
            };

            var results = Validate(user);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(User.PasswordHash)));
        }

        [Fact]
        public void DefaultRole_IsUser()
        {
            var user = new User();
            Assert.Equal("User", user.Role);
        }
    }
}