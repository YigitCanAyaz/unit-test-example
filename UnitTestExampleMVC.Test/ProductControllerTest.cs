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
    public class ProductControllerTest
    {
        private readonly Mock<IRepository<Product>> _mockRepo;
        private readonly ProductsController _productsController;
        private List<Product> _products;

        public ProductControllerTest()
        {
            _mockRepo = new Mock<IRepository<Product>>();
            _productsController = new ProductsController(_mockRepo.Object);
            _products = new List<Product>() {
                new Product { Id = 1, Name = "Kalem", Price = 100, Stock = 50, Color = "Kırmızı" },
                new Product { Id = 2, Name = "Defter", Price = 200, Stock = 500, Color = "Mavi" } };
        }

        [Fact]
        public async void Index_ActionExecutes_ReturnView()
        {
            var result = await _productsController.Index();

            Assert.IsType<ViewResult>(result); // dönen değer view result mı?
        }

        [Fact]
        public async void Index_ActionExecutes_ReturnProductList()
        {
            // taklit edeceğimiz kodu yazıyoruz
            _mockRepo.Setup(repo => repo.GetAll()).ReturnsAsync(_products);

            // Index metodu GetAll kullandığı için taklit kodumuz çalışacak
            var result = await _productsController.Index();

            var viewResult = Assert.IsType<ViewResult>(result); // ilk kontrol (view result dönüyor mu)

            // viewResult'ın modeli bir Product nesnesi mi, bunu kontrol ediyoruz
            var productList = Assert.IsAssignableFrom<IEnumerable<Product>>(viewResult.Model); // List olduğundan dolayı IEnumberable olarak dönmesini sağlıyoruz.

            Assert.Equal<int>(2, productList.Count()); // birbirlerine eşitler mi kontrol edelim.
        }

        [Fact]
        public async void Details_IdIsNull_ReturnRedirectToIndexAction()
        {
            var result = await _productsController.Details(null);

            var redirect = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("Index", redirect.ActionName);
        }

        [Fact]
        public async void Details_IdInvalid_ReturnNotFound()
        {
            Product product = null;
            _mockRepo.Setup(repo => repo.GetById(0)).ReturnsAsync(product);

            var result = await _productsController.Details(0);

            var redirect = Assert.IsType<NotFoundResult>(result);

            Assert.Equal<int>(404, redirect.StatusCode);
        }

        [Theory]
        [InlineData(1)]
        public async void Details_ValidId_ReturnProduct(int productId)
        {
            Product product = _products.First(x => x.Id == productId);

            // veritabanına bağlanmak yerine işlemimizi local hallettik
            _mockRepo.Setup(repo => repo.GetById(productId)).ReturnsAsync(product);

            // bu kod yukarıdaki sahte kodu çalıştıracak
            var result = await _productsController.Details(productId);

            var viewResult = Assert.IsType<ViewResult>(result);

            var resultProduct = Assert.IsAssignableFrom<Product>(viewResult.Model);

            Assert.Equal(product.Id, resultProduct.Id);
            Assert.Equal(product.Name, resultProduct.Name);
        }

        [Fact]
        public void Create_ActionExecutes_ReturnView()
        {
            var result = _productsController.Create();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async void CreatePOST_InvalidModelState_ReturnView()
        {
            _productsController.ModelState.AddModelError("Name", "Name alanı gereklidir");

            var result = await _productsController.Create(_products.First());

            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.IsType<Product>(viewResult.Model);
        }

        [Fact]
        public async void CreatePOST_ValidModelState_ReturnRedirectToIndexAction()
        {
            var result = await _productsController.Create(_products.First());

            var redirect = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("Index", redirect.ActionName);
        }

        [Fact]
        public async void CreatePOST_ValidModelState_CreateMethodExecute()
        {
            Product newProduct = null;
            _mockRepo.Setup(repo => repo.Create(It.IsAny<Product>()))
                .Callback<Product>(x => newProduct = x);

            // bu kod çalışırken sahte Create metoduna gelecek, daha sonra verdiğim product nesnesini yakalayıp newProduct'ı veriyorum.
            var result = await _productsController.Create(_products.First());

            // Create metodu 1 kez çalıştıysa aşağıdaki verify başarılı olur
            _mockRepo.Verify(repo => repo.Create(It.IsAny<Product>()), Times.Once);

            Assert.Equal(_products.First().Id, newProduct.Id);
        }

        [Fact]
        public async void CreatePOST_InvalidModelState_NeverCreateExecute()
        {
            _productsController.ModelState.AddModelError("Name", "");

            var result = await _productsController.Create(_products.First());

            _mockRepo.Verify(repo => repo.Create(It.IsAny<Product>()), Times.Never);
        }

        [Fact]
        public async void Edit_IdIsNull_ReturnRedirectToIndexAction()
        {
            var result = await _productsController.Edit(null);

            var redirect = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("Index", redirect.ActionName);
        }

        [Theory]
        [InlineData(3)]
        public async void Edit_IdInvalid_ReturnNotFound(int productId)
        {
            Product product = null;
            _mockRepo.Setup(repo => repo.GetById(productId)).ReturnsAsync(product);

            var result = await _productsController.Edit(productId);

            var redirect = Assert.IsType<NotFoundResult>(result);

            Assert.Equal<int>(404, redirect.StatusCode); // bunu yazmasakta olur
        }

        [Theory]
        [InlineData(2)]
        public async void Edit_ActionExecutes_ReturnProduct(int productId)
        {
            Product product = _products.First(x => x.Id == productId);

            _mockRepo.Setup(repo => repo.GetById(productId)).ReturnsAsync(product);

            var result = await _productsController.Edit(productId);

            var viewResult = Assert.IsType<ViewResult>(result);

            var resultProduct = Assert.IsAssignableFrom<Product>(viewResult.Model);

            Assert.Equal(product.Id, resultProduct.Id);
            Assert.Equal(product.Name, resultProduct.Name);
        }

        [Theory]
        [InlineData(1)]
        public void EditPOST_IdIsNotEqualProduct_ReturnNotFound(int productId)
        {
            var result = _productsController.Edit(2, _products.First(x => x.Id == productId));

            var redirect = Assert.IsType<NotFoundResult>(result);
        }

        [Theory]
        [InlineData(1)]
        public void EditPOST_InvalidModelState_ReturnView(int productId)
        {
            _productsController.ModelState.AddModelError("Name", "");

            var result = _productsController.Edit(productId, _products.First(x => x.Id == productId));

            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.IsType<Product>(viewResult.Model);
        }

        [Theory]
        [InlineData(1)]
        public void EditPOST_ValidModelState_ReturnRedirectToIndexAction(int productId)
        {
            var result = _productsController.Edit(productId, _products.First(x => x.Id == productId));

            var redirect = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("Index", redirect.ActionName);
        }

        [Theory]
        [InlineData(1)]
        public void EditPOST_ValidModelState_UpdateMethodExecute(int productId)
        {
            var product = _products.First(x => x.Id == productId);

            _mockRepo.Setup(repo => repo.Update(product));

            _productsController.Edit(productId, product);

            _mockRepo.Verify(repo => repo.Update(It.IsAny<Product>()), Times.Once);
        }

        [Fact]
        public async void Delete_IdIsNull_ReturnNotFound()
        {
            var result = await _productsController.Delete(null);

            Assert.IsType<NotFoundResult>(result);
        }

        [Theory]
        [InlineData(0)]
        public async void Delete_IdIsNotEqualProduct_ReturnNotFound(int productId)
        {
            Product product = null;
            _mockRepo.Setup(repo => repo.GetById(productId)).ReturnsAsync(product);

            var result = await _productsController.Delete(productId);

            Assert.IsType<NotFoundResult>(result);
        }

        [Theory]
        [InlineData(1)]
        public async void Delete_ActionExecutes_ReturnProduct(int productId)
        {
            var product = _products.First(x => x.Id == productId);

            _mockRepo.Setup(repo => repo.GetById(productId)).ReturnsAsync(product);

            var result = await _productsController.Delete(productId);

            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.IsAssignableFrom<Product>(viewResult.Model);
        }

        [Theory]
        [InlineData(1)]
        public async void DeleteConfirmed_ActionExecutes_ReturnRedirectToIndexAction(int productId)
        {
            var result = await _productsController.DeleteConfirmed(productId);

            Assert.IsType<RedirectToActionResult>(result);
        }

        [Theory]
        [InlineData(1)]
        public async void DeleteConfirmed_ActionExecutes_DeleteMethodExecute(int productId)
        {
            var product = _products.First(x => x.Id == productId);
            _mockRepo.Setup(repo => repo.Delete(product)); // void olduğu için geriye bir şey dönmüyoruz

            await _productsController.DeleteConfirmed(productId);
            _mockRepo.Verify(repo => repo.Delete(It.IsAny<Product>()), Times.Once);
        }

        [Theory]
        [InlineData(1)]
        public void ProductExists_IdIsNotEqualProduct_ReturnFalse(int productId)
        {
            Product product = null;
            _mockRepo.Setup(repo => repo.GetById(productId)).ReturnsAsync(product);

            var result = _productsController.ProductExists(productId);

            Assert.False(result);
        }

        [Theory]
        [InlineData(1)]
        public void ProductExists_ActionExecutes_ReturnTrue(int productId)
        {
            Product product = _products.First(x => x.Id == productId);
            _mockRepo.Setup(repo => repo.GetById(productId)).ReturnsAsync(product);

            var result = _productsController.ProductExists(productId);

            Assert.True(result);
        }

    }
}
