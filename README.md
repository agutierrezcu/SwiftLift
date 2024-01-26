# SwiftLift

*"swift" with "lift" suggests quick and efficient transportation, indicating a commitment to providing timely and reliable rides.*

- TODO
    - Refactor Guard clauses within async methods
    
    - Check .editorconfig rules to set suffix Async for async methods
    https://medium.com/@asad99/enforcing-async-method-naming-convention-using-editorconfig-in-visual-studio-829d358edb28

Architectural / Design  / Implementation considerations

- Aspire as startup project template

- Add .editorconfig rules (same as Aspire)

- Add code coverage and reporting generator
    - Use local dotnet tools

- Manage packages versions centrally for
    - Services and shared projects
    - And unit tests projects

- Integrate with application insight
    - Enable Azure Monitor  
	- Add connection string environment checker

 - Add Scrutor package
   - Handle auto dependencies registration by assemblies scanning
   - Apply decorators

 - Add snake json serializer / deserializer abstractions
    - And their implementations base on System.Text.Json

 - Add FluentValidation package to perform validations


- VS2022 IDE Extensions

Fine Code Coverage
Code Clean Up on Save
