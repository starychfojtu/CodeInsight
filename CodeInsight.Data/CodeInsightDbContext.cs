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

        public DbSet<Commit.Commit> Commits { get; private set; }

        public DbSet<Issue.Issue> Issues { get; private set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<JobExecution.JobExecution>().HasKey(r => r.Id);
            modelBuilder.Entity<Repository.Repository>().HasKey(r => r.Id);
            
            modelBuilder.Entity<PullRequest.PullRequest>().HasKey(r => r.Id);
            modelBuilder.Entity<PullRequest.PullRequest>().HasOne<Repository.Repository>().WithMany().HasForeignKey(pr => pr.RepositoryId);

            modelBuilder.Entity<Commit.Commit>().HasKey(c => c.Id);
            modelBuilder.Entity<Commit.Commit>().HasOne<Repository.Repository>().WithMany().HasForeignKey(c => c.RepositoryId);

            modelBuilder.Entity<Issue.Issue>().HasKey(i => i.Id);
            modelBuilder.Entity<Issue.Issue>().HasOne<Repository.Repository>().WithMany().HasForeignKey(i => i.RepositoryId);
        }
    }
}