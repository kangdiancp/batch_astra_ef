using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using static System.Console;
using Microsoft.EntityFrameworkCore;
using EFCore_NortwindDb;
using Microsoft.EntityFrameworkCore.Storage;

namespace EFCore_NorthwindApp
{
    class Program
    {
        private static IConfigurationRoot Configuration;
        private static DbContextOptionsBuilder<NorthwindContext> optionsBuilder;
        static void Main(string[] args)
        {
            BuildConfiguration();
            Console.WriteLine($"ConnectionString : {Configuration.GetConnectionString("NorthWindDS")}");
            BuildOptions();


            //ListCustomer();
            //InsertCategory();
            //UpdateCategory();
            //DeleteCategory();
            //FilteringAndSorting();
            //SelectFilteringSorting();
            //JoiningCategoriesAndProduct();
            //JoinGroupCategoryAndProduct();
            //JoinIncludeCategoryProduct();
            //AggregateProduct();

            GetProductSupplierDto();
        }

        static void BuildConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            Configuration = builder.Build();
        }

        static void BuildOptions()
        {
            optionsBuilder = new DbContextOptionsBuilder<NorthwindContext>();
            optionsBuilder.UseSqlServer(Configuration.GetConnectionString("NorthWindDS"));
        }

        static void ListCustomer()
        {
            using (var db = new NorthwindContext(optionsBuilder.Options))
            {
                var customers = db.Customers.OrderByDescending(x => x.CompanyName).Take(10).ToList();

                foreach (var cust in customers)
                {
                    Console.WriteLine($"{cust.CompanyName} {cust.ContactName}");
                }
            }
        }

        static void InsertCategory()
        {
            using (var db = new NorthwindContext(optionsBuilder.Options))
            {
                var isCategoryExist = db.Categories.Find(10);
                if (isCategoryExist == null)
                {
                    var category = new Category()
                    {
                        CategoryName = "Handphone",
                        Description = "Samsung A5"
                    };

                    db.Categories.Add(category);
                    db.SaveChanges();

                }
            }
        } 

        static void UpdateCategory()
        {
            using (var db = new NorthwindContext(optionsBuilder.Options))
            {
                // search category by id
                var categoryExist = db.Categories.Find(11);
                
                if (categoryExist != null)
                {
                    categoryExist.CategoryName = "TV";
                    categoryExist.Description = "TV Samsung";

                    db.Categories.Update(categoryExist);
                    db.SaveChanges();
                }
            }
        }

        static void DeleteCategory()
        {
            using (var db = new NorthwindContext(optionsBuilder.Options))
            {
                
                // dbtransaction
                using (IDbContextTransaction t = db.Database.BeginTransaction())
                {
                    var categoryRemove = db.Categories.Find(11);
                    db.Categories.Remove(categoryRemove);
                    db.SaveChanges();
                    t.Commit();

                }
            }
        }

        private static void FilteringAndSorting()
        {
            using (var db = new NorthwindContext(optionsBuilder.Options))
            {
                IEnumerable<Product> filterProduct = db.Products.Where(x => x.UnitPrice < 10M);

                filterProduct = filterProduct.OrderByDescending(x => x.UnitPrice);

                WriteLine("Product price less than $10j :");
                foreach (var product in filterProduct)
                {
                    WriteLine("{0} : {1} cost {2:$#,##0.00}",
                        product.ProductId,product.ProductName,product.UnitPrice);
                }
                WriteLine();

            }
        }

        private static void SelectFilteringSorting()
        {
            using (var db = new NorthwindContext(optionsBuilder.Options))
            {

                // Interface IQuery filtering dilakukan di client side
                IOrderedQueryable<Product> sortFilteredProducts = db.Products.OrderByDescending(p => p.UnitPrice);

                var attrProducts = sortFilteredProducts
                    .Select(product => new
                    {
                        prodId = product.ProductId,
                        product.ProductName,
                        product.UnitPrice,
                        total = product.UnitPrice * product.UnitsInStock
                    });


                WriteLine("Product price less than $10 :");
                foreach (var product in attrProducts)
                {
                    WriteLine("{0} : {1} cost {2:$#,##0.00} {3:$#,##0.00},",
                        product.prodId, product.ProductName, product.UnitPrice,product.total);
                }
                WriteLine();

            }
        }

        
        //join table category dan product
       private static void JoiningCategoriesAndProduct()
        {
            using (var db = new NorthwindContext(optionsBuilder.Options))
            {
                // join dua table categories and produrct
                var queryJoin = db.Categories.Join(
                        inner: db.Products,
                        outerKeySelector: category => category.CategoryId,
                        innerKeySelector: product => product.CategoryId,
                        resultSelector: (c, p) => new { p.ProductId, p.ProductName, c.CategoryName });


                foreach (var item in queryJoin)
                {
                    WriteLine($"{item.ProductId} | {item.ProductName} | {item.CategoryName}");
                }
            }
        }

        // group all product by category
        private static void JoinGroupCategoryAndProduct()
        {
            using (var db = new NorthwindContext(optionsBuilder.Options))
            {
                // grouping two query between category
                var queryGroup = db.Categories.AsEnumerable().GroupJoin(
                        inner : db.Products,
                        outerKeySelector : category => category.CategoryId,
                        innerKeySelector : product => product.CategoryId,
                        resultSelector : (c,matchingProduct) => new
                        {
                            c.CategoryName,
                            Products = matchingProduct.OrderBy(p => p.ProductName)
                        }
                    );

                // display group
                foreach (var category in queryGroup)
                {
                    WriteLine($"{category.CategoryName}");
                    WriteLine("------------------------------");
                    foreach (var product in category.Products)
                    {
                        WriteLine($"{product.ProductName}");
                    }
                }
            }
        }
        
        private static void JoinIncludeCategoryProduct()
        {
            using (var db = new NorthwindContext(optionsBuilder.Options))
            {
                //AsNoTracking digunakan jika records hanya ditampilkan saja, tidak di marker sbg record yg akan di update/delete
                var queryJoin = db.Products.Include(p => p.Category).Include(p => p.Supplier).AsNoTracking().ToList();

                foreach (var item in queryJoin)
                {
                    WriteLine($"{item.ProductId} | {item.ProductName} | {item.Category?.CategoryName} | {item.Supplier?.CompanyName}");
                }
            }
        }

        private static void AggregateProduct()
        {
            using (var db = new NorthwindContext(optionsBuilder.Options))
            {
                WriteLine($"Total Price : {db.Products.Sum(p => p.UnitPrice)}");
            }
        }

        private static void GetProductSupplierDto()
        {
            using (var db = new NorthwindContext(optionsBuilder.Options))
            {
                var resultSet = db.ProductSupplierDtos
                                .FromSqlRaw("SELECT [p].[ProductId], [p].[ProductName],  [c].[CategoryName],  [s].[CompanyName],  [s].[Address] " +
                                            "FROM[Products] AS[p] " +
                                            "LEFT JOIN[Categories] AS[c] ON[p].[CategoryId] = [c].[CategoryId] " +
                                            "LEFT JOIN[Suppliers] AS[s] ON[p].[SupplierId] = [s].[SupplierId]")
                                .ToList();
                
                foreach (var item in resultSet)
                {
                    WriteLine($"{item.productId} | {item.productName} | {item.categoryName} | {item.address}");
                }
            }
        }

    }
}