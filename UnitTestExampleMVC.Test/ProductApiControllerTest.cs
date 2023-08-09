using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitTestExampleMVC.Web.Controllers;
using UnitTestExampleMVC.Web.Helpers;
using UnitTestExampleMVC.Web.Models;
using UnitTestExampleMVC.Web.Repository;
using Xunit;

namespace UnitTestExampleMVC.Test
{
    public class ProductApiControllerTest
    {
        private readonly Mock<IRepository<Product>> _mockRepo;
        private readonly ProductsApiController _controller;
        private readonly Helper _helper;

        private List<Product> _products;

        public ProductApiControllerTest()
        {
            _mockRepo = new Mock<IRepository<Product>>();
            _controller = new ProductsApiController(_mockRepo.Object);
            _helper = new Helper();
            _products = new List<Product>() {
                new Product { Id = 1, Name = "Kalem", Price = 100, Stock = 50, Color = "Kırmızı" },
                new Product { Id = 2, Name = "Defter", Price = 200, Stock = 500, Color = "Mavi" } };
        }

        [Theory]
        [InlineData(4, 5, 9)]
        public void Add(int a, int b, int total)
        {
            var result = _helper.Add(a, b);
            Assert.Equal(total, result);
        }

        [Fact]
        public async void GetProduct_ActionExecutes_ReturnOkResultWithProducts()
        {
            _mockRepo.Setup(repo => repo.GetAll()).ReturnsAsync(_products);

            var result = await _controller.GetProduct();

            var okResult = Assert.IsType<OkObjectResult>(result);

            var returnProducts = Assert.IsAssignableFrom<IEnumerable<Product>>(okResult.Value);

            Assert.Equal<int>(2, returnProducts.ToList().Count);
        }

        [Theory]
        [InlineData(0)]
        public async void GetProduct_IdInvalid_ReturnNotFound(int productId)
        {
            Product product = null;

            _mockRepo.Setup(repo => repo.GetById(productId)).ReturnsAsync(product);

            var result = await _controller.GetProduct(productId);

            Assert.IsType<NotFoundResult>(result);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async void GetProduct_IdValid_ReturnOkResult(int productId)
        {
            var product = _products.First(x => x.Id == productId);

            _mockRepo.Setup(x => x.GetById(productId)).ReturnsAsync(product);

            var result = await _controller.GetProduct(productId);

            var okResult = Assert.IsType<OkObjectResult>(result);

            var returnProduct = Assert.IsType<Product>(okResult.Value);

            Assert.Equal(productId, returnProduct.Id);
            Assert.Equal(product.Name, returnProduct.Name);
        }

        [Theory]
        [InlineData(1)]
        public async void PutProduct_IdIsNotEqualProduct_ReturnBadRequestResult(int productId)
        {
            var product = _products.First(x => x.Id != productId);

            var result = await _controller.PutProduct(productId, product);

            var badRequestResult = Assert.IsType<BadRequestResult>(result);
        }

        [Theory]
        [InlineData(1)]
        public async void PutProduct_ActionExecutes_ReturnNoContent(int productId)
        {
            var product = _products.First(x => x.Id == productId);

            _mockRepo.Setup(x => x.Update(product));

            var result = await _controller.PutProduct(productId, product);

            _mockRepo.Verify(repo => repo.Update(product), Times.Once);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async void PostProduct_ActionExecutes_ReturnCreatedAtAction()
        {
            var product = _products.First();

            _mockRepo.Setup(x => x.Create(product)).Returns(Task.CompletedTask);

            var result = await _controller.PostProduct(product);

            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);

            _mockRepo.Verify(repo => repo.Create(product), Times.Once);

            Assert.Equal("GetProduct", createdAtActionResult.ActionName);
        }

        [Theory]
        [InlineData(0)]
        public async void DeleteProduct_IdInvalid_ReturnNotFound(int productId)
        {
            Product product = null;

            _mockRepo.Setup(repo => repo.GetById(productId)).ReturnsAsync(product);

            var resultNotFound = await _controller.DeleteProduct(productId);

            Assert.IsType<NotFoundResult>(resultNotFound);
        }

        [Theory]
        [InlineData(1)]
        public async void DeleteProduct_ActionExecute_ReturnNoContent(int productId)
        {
            var product = _products.First(x => x.Id == productId);

            _mockRepo.Setup(repo => repo.GetById(productId)).ReturnsAsync(product);

            _mockRepo.Setup(repo => repo.Delete(product));

            var noContentResult = await _controller.DeleteProduct(productId);

            _mockRepo.Verify(x => x.Delete(product), Times.Once);

            Assert.IsType<NoContentResult>(noContentResult);
        }
    }
}
