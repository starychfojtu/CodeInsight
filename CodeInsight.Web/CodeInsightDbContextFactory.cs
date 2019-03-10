using System;
using CodeInsight.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CodeInsight.Web
{
    public sealed class CodeInsightDbContextFactory : IDesignTimeDbContextFactory<CodeInsightDbContext>
    {
        public CodeInsightDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<CodeInsightDbContext>();
            optionsBuilder.UseMySql(
                Environment.GetEnvironmentVariable("JAWSDB_MARIA_URL"),
                o => o.MigrationsAssembly("CodeInsight.Web")
            );
            return new CodeInsightDbContext(optionsBuilder.Options);
        }
    }
}