using Oakton.Environment;
using Serilog.Core;
using SwiftLift.Infrastructure;
using Xunit.Abstractions;

namespace SwiftLift.Architecture.Tests.Infrastructure;

public sealed class InfrastructureProjectTests(ITestOutputHelper output)
{
    private static readonly Types s_infrastructureTypes = Types.InAssembly(typeof(AppDomainExtensions).Assembly);

    [Fact]
    public void All_Classes_Should_Be_Reside_Infrastructure_Namespace_And_Match_Source_File_Name()
    {
        // Act
        var result = s_infrastructureTypes
            .That()
            .ResideInNamespace("SwiftLift")
            .And()
            .AreClasses()
            .And()
            .AreNotNested()
            .Should()
            .ResideInNamespaceMatching("SwiftLift.Infrastructure")
            .And()
            .HaveSourceFileNameMatchingName()
            .GetResult();

        PrintOutIfFail(output, result);

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void All_Classes_Should_Be_Internal_Or_Sealed()
    {
        // Act
        var result = s_infrastructureTypes
            .That()
            .ResideInNamespace("SwiftLift")
            .And()
            .AreClasses()
            .And()
            .AreNotNested()
            .Should()
            .BeStatic()
            .Or()
            .BeSealed()
            .GetResult();

        PrintOutIfFail(output, result);

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Project_Should_Not_Have_Dependency_Other_Than()
    {
        // Act
        var result = s_infrastructureTypes
            .That()
            .ResideInNamespace("SwiftLift")
            .And()
            .AreClasses()
            .And()
            .AreNotNested()
            .ShouldNot()
            .HaveDependencyOtherThan(
                "System", "Microsoft", "Oakton", "Serilog",
                "FluentValidation", "Ardalis", "SwiftLift.Infrastructure",
                "MassTransit", "Murmur", "Scrutor", "FastEndpoints", "Coverlet")
            .GetResult();

        PrintOutIfFail(output, result);

        // Assert   
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void All_LogEventEnrichers_Classes_Should_EndWith_Enricher()
    {
        // Act
        var result = s_infrastructureTypes
            .That()
            .ResideInNamespace("SwiftLift")
            .And()
            .AreClasses()
            .And()
            .AreNotNested()
            .And()
            .ImplementInterface<ILogEventEnricher>()
            .Should()
            .HaveNameEndingWith("Enricher")
            .GetResult();

        PrintOutIfFail(output, result);

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void All_EnvironmentChecks_Classes_Should_EndWith_EnvironmentCheck()
    {
        // Act
        var result = s_infrastructureTypes
            .That()
            .ResideInNamespace("SwiftLift")
            .And()
            .AreClasses()
            .And()
            .AreNotNested()
            .And()
            .ImplementInterface<IEnvironmentCheck>()
            .Should()
            .HaveNameEndingWith("EnvironmentCheck")
            .GetResult();

        PrintOutIfFail(output, result);

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }

    private static void PrintOutIfFail(ITestOutputHelper output, TestResult result)
    {
        if (result.IsSuccessful)
        {
            return;
        }

        output.WriteLine("Total types: {0}", result.LoadedTypes.Count);
        output.WriteLine("Total failed types {0}", result.FailingTypes.Count);
        output.WriteLine(string.Empty);
        output.WriteLine("Rules failed for types:");

        foreach (var failingType in result.FailingTypes)
        {
            output.WriteLine("Type: {0} Explanation: {1}", failingType.FullName, failingType.Explanation);
        }
    }
}
