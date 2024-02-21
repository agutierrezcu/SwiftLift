rmdir /q /s tests\coverage
rmdir /q /s tests\SwiftLift.Architecture.Tests\TestResults
rmdir /q /s tests\SwiftLift.Infrastructure.UnitTests\TestResults
rmdir /q /s tests\SwiftLift.Riders.Api.IntegrationTests\TestResults
rmdir /q /s tests\SwiftLift.SharedKernel.UnitTests\TestResults

dotnet clean
dotnet tool restore
dotnet build -c Release
dotnet tool run coverlet .\tests\SwiftLift.Infrastructure.UnitTests\bin\SwiftLift.Infrastructure.UnitTests.dll --no-build --target "dotnet" --targetargs "test --no-build" --exclude-by-file "**/NetEscapades.EnumGenerators/**/*.cs" --exclude-by-attribute "ExcludeFromCodeCoverage" --exclude-by-attribute "CompilerGenerated" --exclude-by-attribute "Obsolete" --exclude-by-attribute "GeneratedCode"
dotnet test --collect:"XPlat Code Coverage"

dotnet tool run reportgenerator -reports:./tests/*.UnitTests/TestResults/*/*.xml -targetdir:./tests/coverage/report/ -reporttypes:Html_Dark -sourcedirs:./src/ -historydir:./tests/coverage/history

start "" "./tests/coverage/report/index.html"
