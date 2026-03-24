# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Build the solution
dotnet build src/SFA.DAS.Learning.sln

# Run all tests
dotnet test src/SFA.DAS.Learning.sln

# Run a specific test project
dotnet test src/Domain.Tests/SFA.DAS.Learning.Domain.UnitTests.csproj
dotnet test src/SFA.DAS.Learning.Command.UnitTests/SFA.DAS.Learning.Command.UnitTests.csproj
dotnet test src/SFA.DAS.Learning.InnerApi.UnitTests/SFA.DAS.Learning.InnerApi.UnitTests.csproj
dotnet test src/AcceptanceTests/SFA.DAS.Learning.AcceptanceTests.csproj

# Run a single test by name
dotnet test src/SFA.DAS.Learning.sln --filter "FullyQualifiedName~TestMethodName"

# Run the InnerApi
dotnet run --project src/InnerApi/SFA.DAS.Learning.InnerApi.csproj

# Run the Azure Function
dotnet run --project src/Functions/SFA.DAS.Learning.Functions.csproj
```

## Architecture

This repository contains two runnable applications and several supporting libraries, all targeting .NET 8.

### Applications

- **InnerApi** (`src/InnerApi/`) — ASP.NET Core REST API. Handles GET queries and some write operations (price changes, learner updates). Uses `IQueryDispatcher` and `ICommandDispatcher` injected into controllers. Config loaded from Azure Table Storage keys `SFA.DAS.Learning` and `SFA.DAS.Encoding`.

- **Functions** (`src/Functions/`) — Azure Durable Functions app. Listens on NServiceBus (Azure Service Bus) for external events (e.g. `ApprenticeshipCreatedEvent` from Commitments). Maps events to commands and dispatches them.

### Internal Libraries

| Project | Purpose |
|---|---|
| `Domain` | DDD aggregates, domain events, repository interfaces, domain services |
| `Command` | `ICommandDispatcher`, command handlers, DI registration |
| `Queries` | `IQueryDispatcher`, query handlers, DI registration |
| `DataAccess` | EF Core `LearningDataContext`, entity classes, SQL Server |
| `Infrastructure` | Configuration models, external API client (`LearningOuterApiClient`), EF setup |
| `SFA.DAS.Learning.MessageHandlers` | Domain event handlers (internal, not NServiceBus) |
| `SFA.DAS.Learning.Enums` | Shared enums (`FundingType`, `FundingPlatform`, `Milestone`, etc.) |
| `SFA.DAS.Learning.Models` | Update model DTOs used by commands |
| `Types` | NServiceBus integration event types published to the service bus |
| `TestHelpers` | Shared test utilities and builders |

### Domain Model

The core domain follows DDD with aggregate roots inheriting from `AggregateRoot`, which accumulates domain events via `AddEvent()` and flushes them via `FlushEvents()`.

Two aggregate hierarchies exist under `LearningDomainModel<T>`:

- **`ApprenticeshipLearningDomainModel`** — full apprenticeships. Contains `ApprenticeshipEpisodeDomainModel` (with `EpisodePriceDomainModel`, `EpisodeBreakInLearningDomainModel`, `LearningSupportDomainModel`) and `MathsAndEnglishDomainModel`.
- **`ShortCourseLearningDomainModel`** — short courses. Contains `ShortCourseEpisodeDomainModel` (with `ShortCourseMilestoneDomainModel`).

**`LearnerDomainModel`** is a separate aggregate representing the person. Each learning record links to a learner via `LearnerKey`.

Repository implementations (e.g. `LearnerRepository`, `ApprenticeshipLearningRepository`) call `SaveChangesAsync()` then flush and dispatch domain events via `IDomainEventDispatcher`. The `ILearningRepositoryProvider` / `ILearningService` resolve the correct repository type (`ApprenticeshipLearning` vs `ShortCourseLearning`) at runtime.

### CQRS Pattern

- **Commands**: Implement `ICommand` or `ICommand<TResult>`. Handlers implement `ICommandHandler<TCommand>` or `ICommandHandler<TCommand, TResult>`. All command handlers are automatically decorated with `CommandHandlerWithUnitOfWork` (wraps DB save). Handlers are auto-registered via Scrutor assembly scanning.
- **Queries**: Implement `IQuery`. Handlers implement `IQueryHandler<TQuery, TResponse>`. Auto-registered via Scrutor.

### Domain Events vs Integration Events

- **Domain events** (`src/Domain/Events/`) — handled in-process by `IDomainEventDispatcher` routing to `IDomainEventHandler<T>` implementations in `SFA.DAS.Learning.MessageHandlers`. Used for side effects like writing `LearningHistory` records.
- **Integration events** (`src/Types/`) — NServiceBus messages published to Azure Service Bus for consumption by other services (e.g. earnings, payments).

### Data

EF Core with SQL Server. The `LearningDataContext` is configured in `DataAccess/ServiceCollectionExtensions` via `AddEntityFrameworkForApprenticeships`. Managed identity authentication is used outside LOCAL environments via `SqlAzureIdentityAuthenticationDbConnectionInterceptor`. History data is stored in the `History` schema (`LearningHistory` table).

### Testing

- Unit tests use NUnit + Moq + FluentAssertions.
- Acceptance tests use SpecFlow (`.feature` files in `src/AcceptanceTests/Features/`) with NUnit. They spin up the actual Functions and InnerApi in-process against a real SQL Server database. Requires a local SQL Server and Azure Storage emulator (Azurite).
