using System;
using System.Linq;
using System.Threading.Tasks;
using CodeInsight.Github;
using CodeInsight.Github.Queries;
using CodeInsight.Jobs;
using CodeInsight.Jobs.Instances;
using CodeInsight.Library.Extensions;
using CodeInsight.Library.Types;
using CodeInsight.Web.Common;
using CodeInsight.Web.Common.Security;
using CodeInsight.Web.Models.Github;
using FuncSharp;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Octokit;
using Controller = Microsoft.AspNetCore.Mvc.Controller;
using static CodeInsight.Library.Prelude;
using Monad;
using Connection = Octokit.GraphQL.Connection;
using IConnection = Octokit.GraphQL.IConnection;
using ProductHeaderValue = Octokit.ProductHeaderValue;

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
        public Task<IActionResult> ChooseRepository(int? errorCode = null) => ConnectionAction((connection, token) =>
        {
            var errorMessage = GetErrorMessage(errorCode);

            var repositoriesQueryResult = GetAllRepositoriesQuery.Execute(connection).Bind(r1 =>
                GetAllRepositoriesPrivateQuery.Execute(connection).Map(r2 => r1.Concat(r2))
            );

            var result =
                from repositories in repositoriesQueryResult
                let inputs = repositories.Select(r => new RepositoryInputDto(r.Name, r.Owner))
                select (IActionResult) View(new ChooseRepositoryViewModel(inputs, errorMessage));

            return result.Execute();
        });

        private static IOption<string> GetErrorMessage(int? errorCode)
        {
            var error = errorCode.ToOption().FlatMap(e => e.AsStruct<ChooseRepositoryError>());
            return error.FlatMap(e => e.Match(
                ChooseRepositoryError.InvalidNameWithOwner, _ => Some("Please select one of the repositories."),
                _ => None<string>()
            ));
        }

        private enum ChooseRepositoryError
        {
            InvalidNameWithOwner = 0,
            RepositoryNotFound = 1
        }
        
        [HttpPost]
        public Task<IActionResult> ChooseRepository(string nameWithOwner) => ConnectionAction((connection, token) =>
        {
            var result = ParseInput(nameWithOwner)
                .Bind(i => FindRepository(connection, i.owner, i.name))
                .Bind(r => StartImportJob(importerJob, r, token, configuration.ApplicationName));

            return result.Execute().Map(r => r.Match(
                execution => RedirectToAction("ImportStatus", "Github", new {JobId = execution.Id}),
                error => error.Match(
                    ChooseRepositoryError.InvalidNameWithOwner, _ => RedirectToAction("ChooseRepository", new { errorCode = 0 }),
                    ChooseRepositoryError.RepositoryNotFound, _ => (IActionResult)BadRequest()
                )
            ));
        });
        
        private static IO<Task<ITry<(NonEmptyString owner, NonEmptyString name), ChooseRepositoryError>>> ParseInput(string nameWithOwner) =>
            () => ParseNameWithOwner(nameWithOwner)
                .ToTry(_1 => ChooseRepositoryError.InvalidNameWithOwner)
                .Async();
        
        private static IO<Task<ITry<RepositoryDto, ChooseRepositoryError>>> FindRepository(IConnection connection, NonEmptyString owner, NonEmptyString name) =>
            GetRepositoryQuery
                .Execute(connection, owner, name)
                .Map(t => t.Map(r => r.ToTry(_ => ChooseRepositoryError.RepositoryNotFound)));
        
        private static IO<Task<ITry<JobExecution<string>, ChooseRepositoryError>>> StartImportJob(ImporterJob job, RepositoryDto repository, string token, string applicationName) =>
            job.StartNew(token, applicationName, repository.Name, repository.Owner)
                .Map(j => j.ToSuccess<JobExecution<string>, ChooseRepositoryError>().Async());

        private static IOption<(NonEmptyString owner, NonEmptyString name)> ParseNameWithOwner(string nameWithOwner) =>
            from parts in nameWithOwner.AsOption().Map(n => n.Split('/'))
            from owner in parts.ElementAt(index: 0).FlatMap(NonEmptyString.Create)
            from name in parts.ElementAt(index: 1).FlatMap(NonEmptyString.Create)
            select (owner, name);
        
        #endregion

        #region ImportStatus
        
        [HttpGet]
        public Task<IActionResult> GetImportStatus(Guid jobId)
        {
            return jobExecutionRepository
                .Get<string>(jobId)
                .Map(e => e.Match<IActionResult>(
                    execution => Json(new { Progress = execution.Progress }),
                    _ => NotFound()
                ))
                .Execute();
        }
    
        [HttpGet]
        public Task<IActionResult> ImportStatus(Guid jobId)
        {
            return jobExecutionRepository.Get<string>(jobId)
                .Map(e => e.IsFinished ? ProcessFinished(e) : View(new ImportStatusViewModel(e.Id)))
                .Execute()
                .Map(r => r.GetOrElse((IActionResult)NotFound()));
        }
        
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