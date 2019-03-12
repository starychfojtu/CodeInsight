using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeInsight.Github;
using CodeInsight.Github.Import;
using CodeInsight.Github.Queries;
using CodeInsight.Jobs;
using CodeInsight.Jobs.Instances;
using CodeInsight.Library.Extensions;
using CodeInsight.Library.Types;
using CodeInsight.Web.Common;
using CodeInsight.Web.Common.Security;
using CodeInsight.Web.Models.Github;
using FuncSharp;
using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Monad;
using Monad.Parsec;
using Octokit;
using Octokit.GraphQL;
using Controller = Microsoft.AspNetCore.Mvc.Controller;
using static CodeInsight.Library.Prelude;
using Connection = Octokit.GraphQL.Connection;
using IConnection = Octokit.GraphQL.IConnection;
using ProductHeaderValue = Octokit.ProductHeaderValue;
using Try = FuncSharp.Try;

namespace CodeInsight.Web.Controllers
{
    public class GithubController : Controller
    {
        private static readonly string CsrfSessionKey = "GithubCsrf";

        private readonly ApplicationConfiguration configuration;
        private readonly ImporterJob importerJob;
        private readonly IJobExecutionRepository jobExecutionRepository;
        private readonly GitHubClient client;

        public GithubController(ApplicationConfiguration configuration, ImporterJob importerJob, IJobExecutionRepository jobExecutionRepository)
        {
            this.configuration = configuration;
            this.importerJob = importerJob;
            this.jobExecutionRepository = jobExecutionRepository;
            this.client = new GitHubClient(new ProductHeaderValue(configuration.ApplicationName));
        }

        #region SignIn

        public IActionResult SignIn() =>
            CreateOAuthLoginUrl()
                .ToString()
                .Pipe(Redirect);
        
        private Uri CreateOAuthLoginUrl()
        {
            var csrfToken = Guid.NewGuid().ToString();
            HttpContext.Session.Set(CsrfSessionKey, csrfToken);
            
            return client.Oauth.GetGitHubLoginUrl(new OauthLoginRequest(configuration.ClientId)
            {
                Scopes = { "read:org", "repo" },
                State = csrfToken
            });
        }
        
        #endregion
        
        #region Authorize

        public Task<IActionResult> Authorize(string code, string state)
        {
            var session = HttpContext.Session;
            return GetVerifiedCode(code, state, session)
                .Map(GetOAuthAccessToken)
                .Match(
                    tokenTask => tokenTask.Map(token =>
                    {
                        session.Remove(CsrfSessionKey);
                        session.Set(ClientAuthenticator.GithubTokenSessionKey, token);
                        return (IActionResult)RedirectToAction("ChooseRepository");
                    }),
                    _ => RedirectToAction("Index", "Home").Async<RedirectToActionResult, IActionResult>()
                );
        }
        
        private static IOption<NonEmptyString> GetVerifiedCode(string code, string state, ISession session)
        {
            var expectedState = session.Get<string>(CsrfSessionKey).GetOrElse("");
            return NonEmptyString.Create(code).Where(_ => state == expectedState);
        }
        
        private Task<string> GetOAuthAccessToken(NonEmptyString code)
        {
            var request = new OauthTokenRequest(configuration.ClientId, configuration.ClientSecret, code);
            return client.Oauth.CreateAccessToken(request).Map(t => t.AccessToken);
        }
        
        #endregion

        #region ChooseRepository

        [HttpGet]
        public Task<IActionResult> ChooseRepository() => ConnectionAction((connection, _) =>
            GetAllRepositoriesQuery.Get()
                .Map(items => items.Select(i => new RepositoryInputDto(i.Name, i.Owner)))
                .Map(items => new ChooseRepositoryViewModel(items))
                .Map(vm => (IActionResult) View(vm))
                .Execute(connection));
        
        private enum ChooseRepositoryError
        {
            InvalidNameWithOwner,
            RepositoryNotFound
        }
        
        [HttpPost]
        public Task<IActionResult> ChooseRepository2(string nameWithOwner) => ConnectionAction(async (connection, token) =>
        {
            var result = await ParseInput(nameWithOwner)
                .Bind(i => FindRepository(i.owner, i.name))
                .Bind(r => StartImportJob(importerJob, r, token, configuration.ApplicationName))
                .Execute(connection);
            
            return result.Match(
                execution => RedirectToAction("ImportStatus", "Github", new { JobId = execution.Id }),
                error => error.Match(
                    ChooseRepositoryError.InvalidNameWithOwner, _ => RedirectToAction("ChooseRepository", "Github"),
                    ChooseRepositoryError.RepositoryNotFound, _ => RedirectToAction("ChooseRepository", "Github")
                )
            );
        });
        
        private static Reader<IConnection, Task<ITry<(NonEmptyString owner, NonEmptyString name), ChooseRepositoryError>>> ParseInput(string nameWithOwner) =>
            _ => ParseNameWithOwner(nameWithOwner)
                .ToTry(_1 => ChooseRepositoryError.InvalidNameWithOwner)
                .Async();
        
        private static Reader<IConnection, Task<ITry<RepositoryDto, ChooseRepositoryError>>> FindRepository(NonEmptyString owner, NonEmptyString name) =>
            GetRepositoryQuery
                .Get(owner, name)
                .Map(r => r.ToTry(_ => ChooseRepositoryError.RepositoryNotFound));
        
        private static Reader<IConnection, Task<ITry<JobExecution<string>, ChooseRepositoryError>>> StartImportJob(ImporterJob job, RepositoryDto repository, string token, string applicationName) =>
            _ => job.StartNew(token, applicationName, repository.Name, repository.Owner)
                .ToSuccess<JobExecution<string>, ChooseRepositoryError>()
                .Async();

        private static IOption<(NonEmptyString owner, NonEmptyString name)> ParseNameWithOwner(string nameWithOwner)
        {
            var parts = nameWithOwner.Split('/');
            return
                from owner in parts.Get(0).FlatMap(NonEmptyString.Create)
                from name in parts.Get(1).FlatMap(NonEmptyString.Create)
                select (owner, name);
        }
        
        #endregion

        #region ImportStatus

        [HttpGet]
        public IActionResult ImportStatus(Guid jobId) =>
            jobExecutionRepository
                .Get<string>(jobId)
                .Match(
                    job => job.IsFinished 
                        ? ProcessFinished(job) 
                        : View(new ImportStatusViewModel(job.Progress)),
                    _ => NotFound()
                );

        private IActionResult ProcessFinished(JobExecution<string> execution)
        {
            var repositoryId = execution.Result.Get();
            HttpContext.Session.Set(ClientAuthenticator.GithubRepositoryIdSessionKey, repositoryId);
            return RedirectToAction("Index", "PullRequest");
        }

        #endregion
        
        private Task<IActionResult> ConnectionAction(Func<Connection, string, Task<IActionResult>> action)
        {
            var token = HttpContext.Session.Get<string>(ClientAuthenticator.GithubTokenSessionKey);
            return token.Match(
                t => action(new Connection(new Octokit.GraphQL.ProductHeaderValue(configuration.ApplicationName), t), t),
                _ => NotFound().Async<NotFoundResult, IActionResult>()
            );
        }
    }
}