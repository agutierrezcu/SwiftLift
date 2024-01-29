# SwiftLift

*"swift" with "lift" suggests quick and efficient transportation, indicating a commitment to providing timely and reliable rides.*

- TODO
    - Refactor Guard clauses within async methods

    - Make sure telemetry and logs are been stored in application insight

    - Check logs streamed in hosted web app service
    
    - Check .editorconfig rules to set suffix Async for async methods
    https://medium.com/@asad99/enforcing-async-method-naming-convention-using-editorconfig-in-visual-studio-829d358edb28

Architectural / Design  / Implementation considerations

- Aspire as startup project template

- Add .editorconfig using the one from Aspire as starting point

- Use dotnet tools locally

- Manage nuget packages versions centrally

- Integrate with application insight
    - Enable Azure Monitor  
	- Add connection string environment checker

 - Add snake json serializer / deserializer abstractions
    - And their implementations base on System.Text.Json

 - Add several popular packages for different purposes
    - For development
        - Scrutor for handling dependencies discovery registration by scanning assemblies and defining decorators
        - Ardalis.GuardClauses (to fail fast)
        - Oakton parsing and Utilities for command Line
            > dotnet <application.dll> describe
        - Swashbuckle and OpenApi
        - Evaluating
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
            - Lazard as local services bus
            - Microsoft.AspNetCore.HeaderPropagation
            - Serilog.Sinks.Async
            - Serilog.Enrichers.CorrelationId
            - Serilog.Enrichers.Sensitive
            - Serilog.Expressions
    - For testing
        - NetArchTest.eNhancedEdition for architecture conventions
        - Microsoft.AspNetCore.Mvc.Testing for integration testing
        - AutoBogus.Conventions for fake data generation
        - FluentAssertions for assertion evaluation
        - NSubstitute for mocking
        - xUnit as tests runner
        - Coverlet and reportgenerator tool for code coverage and reporting


#### VS2022 IDE Extensions

- Fine Code Coverage
- Code Clean Up on Save
