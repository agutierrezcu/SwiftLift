dotnet clean
dotnet build -c Release
dotnet tool restore
dotnet tool run coverlet .\tests\SwiftLift.SharedKernel.UnitTests\bin\SwiftLift.SharedKernel.UnitTests.dll --target "dotnet" --targetargs "test --no-build" --exclude-by-attribute "ExcludeFromCodeCoverage"
dotnet test --collect:"XPlat Code Coverage"

dotnet tool run reportgenerator -reports:./tests/*.UnitTests/TestResults/*/*.xml -targetdir:./tests/coverage/report/ -reporttypes:Html_Dark -sourcedirs:./src/ -historydir:./tests/coverage/history

start "" "./tests/coverage/report/index.html"
