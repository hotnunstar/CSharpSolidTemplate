using FluentAssertions;
using SOLID_Template.Domain.Entities;

namespace SOLID_Template.Tests.Domain.Entities;

/// <summary>
/// Basic unit tests for Product entity
/// Tests business logic following SOLID principles
/// </summary>
public class BasicProductTests
{
    [Fact]
    public void Product_Should_Initialize_With_Default_Values()
    {
        // Act
        var product = new Product();

        // Assert
        product.Name.Should().Be(string.Empty);
        product.Sku.Should().Be(string.Empty);
        product.Category.Should().Be(string.Empty);
        product.Price.Should().Be(0);
        product.StockQuantity.Should().Be(0);
        product.OrderProducts.Should().NotBeNull().And.BeEmpty();
    }

    [Theory]
    [InlineData("PROD123", true)]
    [InlineData("ABC-123-XYZ", true)]
    [InlineData("123456", true)]
    [InlineData("", false)]
    [InlineData("AB", false)]
    [InlineData("TOOLONGSKUTHATWILLEXCEEDLIMIT123456789012", true)]
    public void IsSkuValid_Should_Validate_Sku_Format(string sku, bool expectedResult)
    {
        // Arrange
        var product = new Product { Sku = sku };

        // Act
        var isValid = product.IsSkuValid();

        // Assert
        isValid.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(5, true)]
    [InlineData(100, true)]
    public void IsAvailable_Should_Check_Stock_Availability(int stock, bool expectedResult)
    {
        // Arrange
        var product = new Product { StockQuantity = stock };

        // Act
        var isAvailable = product.IsAvailable();

        // Assert
        isAvailable.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(100.0, 10.0, 90.0)]
    [InlineData(50.0, 20.0, 40.0)]
    [InlineData(25.0, 0.0, 25.0)]
    public void CalculateDiscountPrice_Should_Apply_Discount_Correctly(decimal price, decimal discountPercentage, decimal expectedPrice)
    {
        // Arrange
        var product = new Product { Price = price };

        // Act
        var discountedPrice = product.CalculateDiscountPrice(discountPercentage);

        // Assert
        discountedPrice.Should().Be(expectedPrice);
    }

    [Theory]
    [InlineData(10, 5, true, 5)]
    [InlineData(10, 10, true, 0)]
    [InlineData(5, 10, false, 5)]
    [InlineData(0, 1, false, 0)]
    public void TryReduceStock_Should_Handle_Stock_Reduction(int initialStock, int quantity, bool expectedResult, int expectedStock)
    {
        // Arrange
        var product = new Product { StockQuantity = initialStock };

        // Act
        var result = product.TryReduceStock(quantity);

        // Assert
        result.Should().Be(expectedResult);
        product.StockQuantity.Should().Be(expectedStock);
    }
}