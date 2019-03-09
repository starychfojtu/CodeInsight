using System;
using CodeInsight.Data;
using CodeInsight.Data.PullRequest;
using CodeInsight.Data.Repository;
using CodeInsight.Domain.PullRequest;
using CodeInsight.Domain.Repository;
using CodeInsight.Github.Import;
using CodeInsight.Library;
using CodeInsight.Library.Types;
using CodeInsight.Web.Common.Security;
using FuncSharp;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

            services.AddTransient<ClientAuthenticator>();
            services.AddTransient<Importer>();
            services.AddTransient<IPullRequestRepository, PullRequestRepository>();
            services.AddTransient<IPullRequestStorage, PullRequestStorage>();
            services.AddTransient<IRepositoryRepository, RepositoryRepository>();
            services.AddTransient<IRepositoryStorage, RepositoryStorage>();
            services.AddSingleton(GetGithubAppConfig().Get(_ => new InvalidOperationException("Invalid app config.")));
            services.AddDbContext<CodeInsightDbContext>(o => o.UseInMemoryDatabase("CodeInsight"));
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
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

        private IOption<Github.ApplicationConfiguration> GetGithubAppConfig() =>
            from name in NonEmptyString.Create(Environment.GetEnvironmentVariable("GITHUB_APP_NAME"))
            from clientId in NonEmptyString.Create(Environment.GetEnvironmentVariable("GITHUB_CLIENT_ID"))
            from clientSecret in NonEmptyString.Create(Environment.GetEnvironmentVariable("GITHUB_CLIENT_SECRET"))
            select new Github.ApplicationConfiguration(name, clientId, clientSecret);
    }
}