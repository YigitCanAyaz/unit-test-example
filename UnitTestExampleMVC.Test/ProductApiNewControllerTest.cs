using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitTestExampleMVC.Web.Controllers;
using UnitTestExampleMVC.Web.Models;
using UnitTestExampleMVC.Web.Repository;
using Xunit;

namespace UnitTestExampleMVC.Test
{
    public class ProductApiNewControllerTest
    {
        private readonly Mock<IRepository<Product>> _repository;
        private readonly ProductsApiController _controller;
        private List<Product> _products;

        public ProductApiNewControllerTest()
        {
            _repository = new Mock<IRepository<Product>>();
            _controller = new ProductsApiController(_repository.Object);
            _products = new List<Product>() {
                new Product { Id = 1, Name = "Kalem", Price = 100, Stock = 50, Color = "Kırmızı" },
                new Product { Id = 2, Name = "Defter", Price = 200, Stock = 500, Color = "Mavi" } };
        }

        [Fact]
        public async void GetProduct_ActionExecutes_ReturnOkWithIEnumerableProduct()
        {
            _repository.Setup(repo => repo.GetAll()).ReturnsAsync(_products);

            var result = await _controller.GetProduct();

            var okResult = Assert.IsType<OkObjectResult>(result);

            var products = Assert.IsAssignableFrom<IEnumerable<Product>>(okResult.Value);

            Assert.Equal(2, products.ToList().Count);
        }

        [Theory]
        [InlineData(1)]
        public async void GetProduct_ProductIsNull_ReturnNotFound(int productId)
        {
            Product product = null;
            _repository.Setup(repo => repo.GetById(productId)).ReturnsAsync(product);

            var result = await _controller.GetProduct(productId);

            Assert.IsType<NotFoundResult>(result);
        }

        [Theory]
        [InlineData(1)]
        public async void GetProduct_ActionExecutes_ReturnOkWithProduct(int productId)
        {
            Product product = _products.First(product => product.Id == productId);

            _repository.Setup(repo => repo.GetById(productId)).ReturnsAsync(product);

            var result = await _controller.GetProduct(productId);

            var okResult = Assert.IsType<OkObjectResult>(result);

            Assert.IsAssignableFrom<Product>(okResult.Value);
        }

        [Theory]
        [InlineData(1)]
        public async void PutProduct_IdInvalid_ReturnBadRequest(int productId)
        {
            Product product = _products.First(product => product.Id != productId);

            var result = await _controller.PutProduct(productId, product);

            Assert.IsType<BadRequestResult>(result);
        }

        [Theory]
        [InlineData(1)]
        public async void PutProduct_ActionExecutes_ReturnNoContent(int productId)
        {
            Product product = _products.First(product => product.Id == productId);

            _repository.Setup(x => x.Update(product));

            var result = await _controller.PutProduct(productId, product);

            _repository.Verify(repo => repo.Update(product), Times.Once);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async void PostProduct_ActionExecutes_ReturnCreatedAtAction()
        {
            Product product = _products.First();
            _repository.Setup(repo => repo.Create(product)).Returns(Task.CompletedTask);

            var result = await _controller.PostProduct(product);

            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);

            _repository.Verify(repo => repo.Create(product), Times.Once);

            Assert.Equal("GetProduct", createdAtActionResult.ActionName);
        }

        [Theory]
        [InlineData(1)]
        public async void DeleteProduct_ProductIsNull_ReturnNotFound(int productId)
        {
            Product product = null;
            _repository.Setup(repo => repo.GetById(productId)).ReturnsAsync(product);

            var result = await _controller.DeleteProduct(productId);

            Assert.IsType<NotFoundResult>(result);
        }

        [Theory]
        [InlineData(1)]
        public async void DeleteProduct_ActionExecutes_ReturnNoContent(int productId)
        {
            Product product = _products.First();

            _repository.Setup(repo => repo.GetById(productId)).ReturnsAsync(product);

            _repository.Setup(repo => repo.Delete(product));

            var result = await _controller.DeleteProduct(productId);

            _repository.Verify(repo => repo.Delete(product), Times.Once);

            Assert.IsType<NoContentResult>(result);
        }
    }
}
