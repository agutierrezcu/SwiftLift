using Microsoft.AspNetCore.Http;
using Oakton.Environment;
using Serilog.Core;
using SwiftLift.Architecture.Tests.Rules;
using SwiftLift.Infrastructure;
using Xunit.Abstractions;

namespace SwiftLift.Architecture.Tests.Infrastructure;

public sealed class InfrastructureProjectTests(ITestOutputHelper output)
{
    private readonly Func<PredicateList> _getInfrastructureTypesPredicateList = () =>
        Types.InAssembly(typeof(AppDomainExtensions).Assembly)
            .That()
            .ResideInNamespace("SwiftLift")
            .And()
            .AreClasses()
            .And()
            .AreNotNested()
            .And()
            .AreNotInGeneratedFile();

    [Fact]
    public void All_Classes_Should_Be_Reside_Infrastructure_Namespace_And_Match_Source_File_Name()
    {
        // Act
        var result = _getInfrastructureTypesPredicateList()
            .Should()
            .ResideInNamespaceMatching("SwiftLift.Infrastructure")
            .And()
            .HaveSourceFileNameMatchingName()
            .And()
            .HaveSourceFilePathMatchingNamespace()
            .GetResult();

        PrintOutIfFail(output, result);

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void All_Classes_Should_Be_Internal_Or_Sealed()
    {
        // Act
        var result = _getInfrastructureTypesPredicateList()
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
        var result = _getInfrastructureTypesPredicateList()
            .ShouldNot()
            .HaveDependencyOtherThan(
                "SwiftLift.Infrastructure", "SwiftLift.SharedKernel",
                "System", "Microsoft",
                "Oakton", "Serilog",
                "FluentValidation", "Ardalis",
                "MassTransit", "Murmur", "Scrutor",
                "FastEndpoints", "Coverlet")
            .GetResult();

        PrintOutIfFail(output, result);

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }

    [Theory]
    [InlineData(typeof(ILogEventEnricher))]
    [InlineData(typeof(IEnvironmentCheck))]
    [InlineData(typeof(IMiddleware))]
    public void All_LogEventEnrichers_Classes_Should_EndWith_Enricher(Type @interface, string? classNameSuffix = null)
    {
        // Act
        var result = _getInfrastructureTypesPredicateList()
            .And()
            .ImplementInterface(@interface)
            .Should()
            .HaveNameEndingWith(classNameSuffix ?? @interface.Name[1..])
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
