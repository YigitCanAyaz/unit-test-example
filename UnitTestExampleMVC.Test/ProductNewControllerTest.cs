using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnitTestExampleMVC.Web.Models;

namespace UnitTestExampleMVC.Test
{
    // EF Core InMemory
    public class ProductNewControllerTest
    {
        protected DbContextOptions<MVCUnitTestDBContext> _contextOptions { get; private set; }

        public void SetContextOptions(DbContextOptions<MVCUnitTestDBContext> contextOptions)
        {
            _contextOptions = contextOptions;
            Seed();
        }

        public void Seed()
        {
            using (MVCUnitTestDBContext context = new MVCUnitTestDBContext(_contextOptions))
            {
                context.Database.EnsureDeleted(); // silindiğinden emin ol
                context.Database.EnsureCreated(); // sıfırdan tekrar oluştur

                context.Category.Add(new Category() { Name = "Kalemler" });
                context.Category.Add(new Category() { Name = "Defterler" });
                context.SaveChanges();

                context.Product.Add(new Product() { CategoryId = 1, Name = "kalem 10", Price = 100, Stock = 100, Color = "Kırmızı" });

                context.Product.Add(new Product() { CategoryId = 1, Name = "kalem 10", Price = 100, Stock = 100, Color = "Mavi" });

                context.SaveChanges();
            }
        }
    }
}
