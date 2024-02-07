# SwiftLift

*"Swift" and "Lift" imply quick and efficient transportation, indicating a commitment to providing timely and reliable rides.*

### System Description

Creating a simplified Uber-like system as a case study to apply what I've learned over the years and what I'm about to learn.

## **First do it, then do it right, then do it better**

## Goals

- Recreate a distributed system.
- Implement Event-Driven Microservices Architecture.
- Apply Domain-Driven Design (DDD).
- Use Command Query Responsibility Segregation (CQRS).
- Utilize gRPC for synchronous communication.
- Implement Request-Response Pattern.
- Apply API versioning.
- Use API Gateway / YARP.
- Use InMemory / Service Bus.
- Implement Inbox / Outbox Pattern.
- Use Saga Pattern.
- Implement Circuit Breaker Pattern.
- Ensure Resiliency and Fault Tolerance.
- Implement Central Configuration Management.
- Design Idempotent Event Consumers.
- Handle Eventual Consistency.
- Use both Relational and Non-Relational Databases.
- Enforce Early Validation with Options Settings.
- Secure Endpoints Based on JWT from Multiple Identity Providers.
- Use Quartz for Scheduled Tasks in Background Services.
- Implement Service Discovery.

## Architectural / Design / Implementation Considerations

- Use Aspire as a Startup Project Template.
- Add .editorconfig Using the Aspire as a Starting Point.
- Use Dotnet Tools Locally.
- Manage NuGet Package Versions Centrally.
- Propagate Correlation ID Across Applications.
- Integrate with Application Insight.
    - Enable Azure Monitor.
    - Add Connection String Environment Checker.
- Use Feature Flags to Manage Both Functional and Technical Requirements.
- Add Snake JSON Serializer / Deserializer Abstractions.
    - And Their Implementations Based on System.Text.Json.
- Consider these popular libraries as part of the solution:
    - Hashids.net / sqids-dotnet.
    - EF Core.
    - SignalR.
    - FastEndpoints.
    - Masstrasit.
    - Wolverine as Mediator or MediatR.
    - SimpleInjector DI.
    - Mapster.
    - Marten as Transactional Document DB and Event Store.
    - FluentResults.Extensions.AspNetCore.
    - StronglyTypedId.
    - Refit.
    - Polly.
    - Lazard as Local Services Bus.
    - HeaderPropagation.

- For Testing:
    - NetArchTest.EnhancedEdition for Architecture Conventions.
    - Microsoft.AspNetCore.Mvc.Testing for Integration Testing.
    - AutoBogus.Conventions for Fake Data Generation.
    - FluentAssertions for Assertion Evaluation.
    - NSubstitute for Mocking.
    - xUnit as Tests Runner.
    - Coverlet and ReportGenerator Tool for Code Coverage and Reporting.
    - TestContainer.

#### VS2022 IDE Extensions

- Fine Code Coverage.
- Code Clean Up on Save.
- Github Copilot.
