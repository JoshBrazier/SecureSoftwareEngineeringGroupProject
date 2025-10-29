using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SecureSoftwareGroupProject.Models;
using Xunit;

public class CustomerReviewTests
{
    [Fact]
    public void CustomerReview_Should_Have_Default_Values()
    {
        // Arrange
        var review = new CustomerReview();

        // Assert
        Assert.NotEqual(Guid.Empty, review.Id);
        Assert.NotNull(review.CustomerName);
        Assert.NotNull(review.ReviewText);
        Assert.True(review.CreatedAtUtc <= DateTime.UtcNow);
        Assert.True(review.UpdatedAtUtc <= DateTime.UtcNow);
    }

    [Fact]
    public void CustomerReview_WithValidData_ShouldPassValidation()
    {
        // Arrange
        var review = new CustomerReview
        {
            CustomerName = "Alice Smith",
            Rating = 4,
            ReviewText = "Excellent service, would recommend!",
            ImagePath = "/images/review1.jpg"
        };

        var context = new ValidationContext(review);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(review, context, results, true);

        // Assert
        Assert.True(isValid);
        Assert.Empty(results);
    }

    [Fact]
    public void CustomerReview_WithInvalidRating_ShouldFailValidation()
    {
        // Arrange
        var review = new CustomerReview
        {
            CustomerName = "Bob",
            Rating = 7, // invalid (out of range)
            ReviewText = "Too high rating"
        };

        var context = new ValidationContext(review);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(review, context, results, true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(results, r => r.ErrorMessage!.Contains("field Rating"));
    }

    [Fact]
    public void CustomerReview_WithMissingFields_ShouldFailValidation()
    {
        // Arrange
        var review = new CustomerReview
        {
            CustomerName = "",
            Rating = 3,
            ReviewText = ""
        };

        var context = new ValidationContext(review);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(review, context, results, true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(CustomerReview.CustomerName)));
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(CustomerReview.ReviewText)));
    }
}