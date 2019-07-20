# Programmer's documentation

# Projects 

- CodeInsight.Commits: Commits specific things like statistics, could be part fo Domain
- CodeInsight.Data: Access to database.
- CodeInsight.Domain: Plain domain objects without dependency on some tools/infrastructure.
- CodeInsight.Github: Specific project for Github related things.
- CodeInsight.Jobs: Background jobs.
- CodeInsight.Library: Common things not specific to CodeInsight, but rather BCL extensions.
- CodeInsight.PullRequests: Pull request specific things like statistics, could be part of Domain.
- CodeInsight.Tests: Tests.
- CodeInsight.Web: Web layer containing Controllers, ViewModels etc.

# Libraries

The main libraries worth mentioning are:
- NodaTime (https://nodatime.org/), which handles time more correctly and is also more type safe
- FuncSharp (https://github.com/siroky/FuncSharp), which brings basic FP concepts to C#
- Csharp monad (https://github.com/louthy/csharp-monad), which fulfills the lack of monads in FuncSharp
- Hangfire (https://www.hangfire.io/), which helps with running background tasks
- Entity Framework Core (https://docs.microsoft.com/en-us/ef/core/), standard ORM for .NET Core, which is used only in Data Layer.
- Octokit (https://github.com/octokit/octokit.graphql.net), SDK fot github API

# Deployment

After commit is pushed, Travis CI builds the branch and runs tests. If he succeedes, Heroku will push the new version to production.

## Web layer 

Web layer is based on ASP.NET Core MVC (https://docs.microsoft.com/en-us/aspnet/core/mvc/overview?view=aspnetcore-2.2).
For views, razor is used with chart.js (https://www.chartjs.org/) library for rendering of charts with ChartJSCore (https://github.com/mattosaurus/ChartJSCore) for easier usage within C#.

## Authentication

Client is represented by following class.

```csharp
public sealed class Client : Coproduct2<GithubRepositoryClient, Unit>
```

So the only implemented on so far is github, represented by:

```csharp
public class GithubRepositoryClient
{
    public GithubRepositoryClient(Connection connection, RepositoryId repositoryId)
    {
        Connection = connection;
        RepositoryId = repositoryId;
    }

    public Connection Connection { get; }

    public RepositoryId RepositoryId { get; }
}
```

They are stored in ASP Session and retrieved via `ClientAuthenticator` class.
For use in controllers, just inherit `AuthorizedController` class and use its `Action` method, which handles authentication for you.

## Data layer

MySQL is used as database, but that is just a minor detail, that the application should not rely on.
Data layer should only leak its abstraction to outer projects.

## Repository lifecycle

When selected by user, repository is imported. Only changes are retrieved and cached in database.
This is done in background with `ImporterJob` but because the `Octokit.GraphQL` does not implement queries on commits to a repository the authors were forced to use `Octokit`'s connection, client and their methods:

```csharp
var github = new GitHubClient(connection);
var commits = await github.Repository.Commit.GetAll(repository.Owner.Value, repository.Name.Value);

foreach (var commit in commits)
{
    var details = await github.Repository.Commit.Get(repository.Owner.Value, repository.Name.Value, commit.Sha);
}
```

# General style of programming

The project is based heavily on functional programming and type safety.

## Error handling

One thing worth mentioning is for example error handling. Usage of exception is discourage.
Instead, it is better to define enum or coproduct for the error:

```csharp
public enum ConfigurationError
{
    InvalidFromDate,
    InvalidToDate,
    ToDateIsAfterFrom,
    ToDateIsAfterTomorrow
}
```

and then used in return type:
```csharp
private static ITry<IntervalStatisticsConfiguration, ConfigurationError> ParseConfiguration(...
```

## Immutability

With FP in mind, classes are always made immutable, like this example:

```csharp
public class Repository
{
    public Repository(RepositoryId id, NonEmptyString name, NonEmptyString owner)
    {
        Id = id;
        Name = name;
        Owner = owner;
    }

    public RepositoryId Id { get; }

    public NonEmptyString Name { get; }

    public NonEmptyString Owner { get; }
}
```

## Null handling

Instead of working with nulls, usage of IOption<T> is in place. It prevents common NPEs. The compiler will force you to handle bot cases.

Side note:
LINQ syntax for all monads, not only Options and Trys is encouraged:
```csharp
private IOption<Github.ApplicationConfiguration> GetGithubAppConfig() =>
    from name in NonEmptyString.Create(Environment.GetEnvironmentVariable("GITHUB_APP_NAME"))
    from clientId in NonEmptyString.Create(Environment.GetEnvironmentVariable("GITHUB_CLIENT_ID"))
    from clientSecret in NonEmptyString.Create(Environment.GetEnvironmentVariable("GITHUB_CLIENT_SECRET"))
    select new Github.ApplicationConfiguration(name, clientId, clientSecret);
```

## IO

Use IO type to indicate impurity of function.
```csharp
IO<Task<Unit>> Update(IEnumerable<PullRequest> pullRequests);
```

## Keep refactoring

Things mentioned above are not present everywhere. Always refactor the code to better when you see it.
