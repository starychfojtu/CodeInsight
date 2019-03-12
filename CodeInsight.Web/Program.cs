using System;
using CodeInsight.Data;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CodeInsight.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateWebHostBuilder(args).Build();

            var relationDatabaseConnectionExists = Environment.GetEnvironmentVariable("JAWSDB_MARIA_URL") != null;
            if (relationDatabaseConnectionExists)
            {
                var dbContext = host.Services.CreateScope().ServiceProvider.GetService<CodeInsightDbContext>();
                dbContext.Database.Migrate();
            }
            
            host.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}