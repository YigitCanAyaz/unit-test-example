﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitTestExampleMVC.Web.Controllers;
using UnitTestExampleMVC.Web.Models;
using Xunit;

namespace UnitTestExampleMVC.Test
{
    public class ProductNewControllerTestWithSqlServerLocalDb : ProductNewControllerTest
    {
        public ProductNewControllerTestWithSqlServerLocalDb()
        {
            var sqlCon = @"Server=(localdb)\MSSQLLocalDB;Database=LocalDbTest,Trusted_Connection=true,MultipleActiveResultSets=true";

            // Hangi database provider'ı kullanacğaımızı belirtiyoruz
            SetContextOptions(new DbContextOptionsBuilder<MVCUnitTestDBContext>()
                .UseSqlServer(sqlCon).Options);
        }

        [Fact]
        public async Task Create_ModelValidProduct_ReturnRedirectToActionWithSaveProduct()
        {
            var newProduct = new Product { Name = "Kalem 30", Price = 200, Stock = 100, Color = "Mavi" };

            using (var context = new MVCUnitTestDBContext(_contextOptions))
            {
                var category = context.Category.First();

                newProduct.CategoryId = category.Id;

                // var repository = new Repository<Product>(context);
                var controller = new ProductsNewController(context);

                var result = await controller.Create(newProduct);

                var redirect = Assert.IsType<RedirectToActionResult>(result);

                Assert.Equal("Index", redirect.ActionName);
            }

            // ayrı bir context açıyorum çünkü ef core, eklenen dataları memory'de track ediyor
            using (var context = new MVCUnitTestDBContext(_contextOptions))
            {
                var product = context.Product.FirstOrDefault(x => x.Name == newProduct.Name);

                Assert.Equal(newProduct.Name, product.Name);
            }
        }

        [Theory]
        [InlineData(1)]
        public async Task DeleteCategory_ExistCategoryId_DeleteAllProducts(int categoryId)
        {
            using (var context = new MVCUnitTestDBContext(_contextOptions))
            {
                var category = await context.Category.FindAsync(categoryId);

                Assert.NotNull(category);

                context.Category.Remove(category);

                context.SaveChanges();
            }

            using (var context = new MVCUnitTestDBContext(_contextOptions))
            {
                var products = await context.Product.Where(x => x.CategoryId == categoryId).ToListAsync();

                Assert.Empty(products);
            }
        }
    }
}