# Programmer's documentation

## Projects 

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

# Web layer 

Web layer is based on ASP.NET Core MVC (https://docs.microsoft.com/en-us/aspnet/core/mvc/overview?view=aspnetcore-2.2).
For views, razor is used with chart.js (https://www.chartjs.org/) library for rendering of charts with ChartJSCore (https://github.com/mattosaurus/ChartJSCore) for easier usage within C#.

# Authentication

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

# General style of programming

The project is based heavily on functional programming and type safety.

## Error handling

One thing worth mentioning is for example error handling. Usage of exception is discourage.
Instead, define enum or coproduct for your error:

```csharp
public enum ConfigurationError
{
    InvalidFromDate,
    InvalidToDate,
    ToDateIsAfterFrom,
    ToDateIsAfterTomorrow
}
```

and return it from function as follows:
```csharp
private static ITry<IntervalStatisticsConfiguration, ConfigurationError> ParseConfiguration(...
```
