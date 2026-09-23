using Microsoft.AspNetCore.Mvc;
using Moq;
using SaleManagement.Api.Controllers;
using SaleManagement.Api.Repositories;
using SaleManagement.Api.Services;
using Xunit;

namespace SaleManagement.Api.Tests.Controllers;

public sealed class ProductsControllerTests
{
    [Fact]
    public async Task GetProductById_ReturnsNotFound_WhenProductDoesNotExist()
    {
        var productRepository = new Mock<IProductRepository>();
        productRepository
            .Setup(repository => repository.GetProductByIdAsync(999))
            .ReturnsAsync((Entities.ProductEntity?)null);
        var storeRepository = new Mock<IStoreRepository>();
        var controller = new ProductsController(
            new ProductsService(productRepository.Object),
            new PricingEngineService(productRepository.Object, storeRepository.Object));

        var result = await controller.GetProductById(999);

        var response = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, response.StatusCode);
        Assert.NotNull(response.Value);
        var payload = response.Value!;
        Assert.False((bool)payload.GetType().GetProperty("success")!.GetValue(payload)!);
        Assert.Equal(
            "Product with ID 999 not found.",
            (string)payload.GetType().GetProperty("message")!.GetValue(payload)!);
        productRepository.Verify(repository => repository.GetProductByIdAsync(999), Times.Once);
    }
}