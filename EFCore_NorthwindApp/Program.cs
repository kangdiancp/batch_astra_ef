using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using EFCore_NortwindDb;


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


        


    }
}