namespace SwiftLift.Infrastructure;

[ExcludeFromCodeCoverage]
public static class AppDomainExtensions
{
    public static Assembly[] GetApplicationAssemblies(this AppDomain appDomain, string @namespace)
    {
        var assembliesSearchPattern = $"{@namespace}*.*.dll";

        var currentAssemblies = appDomain.GetAssemblies();

        var entryAssemblyLocation = Assembly.GetEntryAssembly()?.Location;

        var directoryName = Path.GetDirectoryName(entryAssemblyLocation)!;

        return currentAssemblies
            .Where(a => a.FullName?.StartsWith(@namespace,
                StringComparison.InvariantCultureIgnoreCase) ?? false)
            .Union(Directory
                .EnumerateFiles(directoryName, assembliesSearchPattern)
                .Where(a => currentAssemblies.All(aa => aa.FullName != a))
                .Select(a => Assembly.Load(AssemblyName.GetAssemblyName(a))))
           .Union(new[] { Assembly.GetEntryAssembly() })
           .GroupBy(a => a?.FullName)
           .Select(a => a.First())
           .ToArray()!;
    }
}
