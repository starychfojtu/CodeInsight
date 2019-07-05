using System;
using System.Data.Common;
using CodeInsight.Data;
using CodeInsight.Data.Commit;
using CodeInsight.Data.Issue;
using CodeInsight.Data.JobExecution;
using CodeInsight.Data.PullRequest;
using CodeInsight.Data.Repository;
using CodeInsight.Domain.Commit;
using CodeInsight.Domain.Issue;
using CodeInsight.Domain.PullRequest;
using CodeInsight.Domain.Repository;
using CodeInsight.Github.Import;
using CodeInsight.Jobs;
using CodeInsight.Jobs.Instances;
using CodeInsight.Library;
using CodeInsight.Library.Types;
using CodeInsight.Web.Common.Security;
using FuncSharp;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CodeInsight.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            Env = env;
        }

        public IConfiguration Configuration { get; }
        
        public IHostingEnvironment Env { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = false;
            });
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddHangfire(config =>
            {
                config.UseMemoryStorage();
            });

            services.AddTransient<ClientAuthenticator>();
            services.AddTransient<Importer>();
            services.AddTransient<CommitImporter>();
            services.AddTransient<IssueImporter>();
            services.AddTransient<PullRequestImporter>();
            services.AddTransient<ImporterJob>();
            services.AddTransient<ICommitRepository, CommitRepository>();
            services.AddTransient<ICommitStorage, CommitStorage>();
            services.AddTransient<IIssueRepository, IssueRepository>();
            services.AddTransient<IIssueStorage, IssueStorage>();
            services.AddTransient<IPullRequestRepository, PullRequestRepository>();
            services.AddTransient<IPullRequestStorage, PullRequestStorage>();
            services.AddTransient<IRepositoryRepository, RepositoryRepository>();
            services.AddTransient<IRepositoryStorage, RepositoryStorage>();
            services.AddTransient<IJobExecutionRepository, JobExecutionRepository>();
            services.AddTransient<IJobExecutionStorage, JobExecutionStorage>();
            services.AddSingleton(GetGithubAppConfig().Get(_ => new InvalidOperationException("Invalid app config.")));

            var mysqlConnString = GetMysqlConnectionString();
            services.AddDbContext<CodeInsightDbContext>(o =>
            {
                mysqlConnString.Match(
                    conn => o.UseMySql(conn),
                    _ => o.UseInMemoryDatabase("CodeInsight")
                );
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseDeveloperExceptionPage();
                //app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            GlobalConfiguration.Configuration.UseActivator(new HangfireActivator(serviceProvider));
            app.UseHangfireDashboard();
            app.UseHangfireServer();
            
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSession();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        private IOption<string> GetMysqlConnectionString() =>
            Environment.GetEnvironmentVariable("JAWSDB_MARIA_URL").ToOption();
        
        private IOption<Github.ApplicationConfiguration> GetGithubAppConfig() =>
            from name in NonEmptyString.Create(Environment.GetEnvironmentVariable("GITHUB_APP_NAME"))
            from clientId in NonEmptyString.Create(Environment.GetEnvironmentVariable("GITHUB_CLIENT_ID"))
            from clientSecret in NonEmptyString.Create(Environment.GetEnvironmentVariable("GITHUB_CLIENT_SECRET"))
            select new Github.ApplicationConfiguration(name, clientId, clientSecret);
    }
}