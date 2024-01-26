namespace SwiftLift.SharedKernel;

[ExcludeFromCodeCoverage]
public static class AppDomainExtensions
{
    public static Assembly[] GetApplicationAssemblies(this AppDomain appDomain, string applicationId)
    {
        var assembliesSearchPattern = $"{applicationId}*.*.dll";

        var currentAssemblies = appDomain.GetAssemblies();

        var entryAssemblyLocation = Assembly.GetEntryAssembly()?.Location;

        var directoryName = Path.GetDirectoryName(entryAssemblyLocation)!;

        return currentAssemblies
            .Where(a => a.FullName?.StartsWith(applicationId, StringComparison.InvariantCultureIgnoreCase) ?? false)
            // http://simpleinjector.readthedocs.io/en/latest/assembly-loading-resolution-conflicts.html
            .Union(Directory
                   .EnumerateFiles(directoryName, assembliesSearchPattern)
                   .Where(a => !currentAssemblies.Any(aa => aa.FullName == a))
                   .Select(a => Assembly.Load(AssemblyName.GetAssemblyName(a))))
           .Union(new[] { Assembly.GetEntryAssembly() })
           .GroupBy(a => a?.FullName)
           .Select(a => a.First())
           .ToArray()!;
    }
}
