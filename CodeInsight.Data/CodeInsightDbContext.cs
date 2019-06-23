using Microsoft.EntityFrameworkCore;

namespace CodeInsight.Data
{
    public sealed class CodeInsightDbContext : DbContext
    {
        public CodeInsightDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<JobExecution.JobExecution> JobExecutions { get; private set; }
        
        public DbSet<Repository.Repository> Repositories { get; private set; }
        
        public DbSet<PullRequest.PullRequest> PullRequests { get; private set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<JobExecution.JobExecution>().HasKey(r => r.Id);
            modelBuilder.Entity<Repository.Repository>().HasKey(r => r.Id);
            
            modelBuilder.Entity<PullRequest.PullRequest>().HasKey(r => r.Id);
            modelBuilder.Entity<PullRequest.PullRequest>().HasOne<Repository.Repository>().WithMany().HasForeignKey(pr => pr.RepositoryId);
            //TODO: Edit accordingly ~ add commits
        }
    }
}