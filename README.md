# SwiftLift

*"swift" with "lift" suggests quick and efficient transportation, indicating a commitment to providing timely and reliable rides.*

### System description

Building a simple tiny Uber as case study to put in practice what I've learned in the last years and what I am abut to learn.


## **First do it, then do it right, then do it better**

## Goals

- Recreate a distributed system.
- Event Driven microservices architecture.
- Domain Driven Design (DDD)
- Command Query Responsibility Segregation (CQRS)
- Grpc for synchronous communication
- Request-Response Pattern.
- Api versioning
- Api getway / YARP
- InMemory / Service Bus
- Inbox / Outbox Pattern.
- Saga Pattern
- Circuit breaker Pattern
- Resiliency and fault tolerance
- Central configuration management
- Design idempotency event consumers
- Deal with eventual consistency
- Relational and No DDBB
- Options settings enforcing early validation
- Secure endpoints based on JWT from multiple identity providers
- Quartz for sheduled tasks in background services
- Service discovery

## Architectural / Design  / Implementation considerations

- Aspire as startup project template

- Add .editorconfig using the one from Aspire as starting point

- Use dotnet tools locally

- Manage nuget package versions centrally

- Propagate correlation id across applications

- Integrate with Aapplication Insight
    - Enable Azure Monitor  
	- Add connection string environment checker

- Using feature flags for manage both functional and technical requirements

- Add snake json serializer / deserializer abstractions
   - And their implementations base on System.Text.Json

 - Popular solutions to be considered as candidates
     - Hashids.net / sqids-dotnet
     - EF Core
     - SignalR
     - FastEndpoints
     - Masstrasit
     - Wolverine as mediator or MediatR
     - SimpleInjector DI
     - Mapster
     - Marten as transactional document DB and event store
     - SmartEnum
     - FluentResults.Extensions.AspNetCore
     - StronglyTypeId
     - Refit
     - Polly

     - Lazard as local services bus
     - HeaderPropagation
     
 - For testing
     - NetArchTest.eNhancedEdition for architecture conventions
     - Microsoft.AspNetCore.Mvc.Testing for integration testing
     - AutoBogus.Conventions for fake data generation
     - FluentAssertions for assertion evaluation
     - NSubstitute for mocking
     - xUnit as tests runner
     - Coverlet and reportgenerator tool for code coverage and reporting
     - TestContainer


#### VS2022 IDE Extensions

- Fine Code Coverage
- Code Clean Up on Save
- Github Copilot
