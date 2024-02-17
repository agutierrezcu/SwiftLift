rmdir /q /s tests\coverage
rmdir /q /s tests\SwiftLift.Architecture.Tests\TestResults
rmdir /q /s tests\SwiftLift.Infrastructure.UnitTests\TestResults
rmdir /q /s tests\SwiftLift.Riders.Api.IntegrationTests\TestResults
rmdir /q /s tests\SwiftLift.SharedKernel.UnitTests\TestResults

dotnet clean
dotnet build -c Release
dotnet tool restore
dotnet tool run coverlet .\tests\SwiftLift.Infrastructure.UnitTests\bin\SwiftLift.Infrastructure.UnitTests.dll --target "dotnet" --targetargs "test --no-build" --exclude-by-attribute "ExcludeFromCodeCoverage"
dotnet test --collect:"XPlat Code Coverage"

dotnet tool run reportgenerator -reports:./tests/*.UnitTests/TestResults/*/*.xml -targetdir:./tests/coverage/report/ -reporttypes:Html_Dark -sourcedirs:./src/ -historydir:./tests/coverage/history

start "" "./tests/coverage/report/index.html"
