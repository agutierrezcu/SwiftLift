using BenchmarkDotNet.Attributes;

namespace SwiftLift.Benchmarks.Enums;

[MemoryDiagnoser(false)]
[HideColumns("Job", "RatioSD", "Error")]
public class Benchmarks
{
    private readonly DayOfWeekEnum _mondayEnum = DayOfWeekEnum.Monday;

    private readonly DayOfWeekSmartEnum _mondaySmart = DayOfWeekSmartEnum.Monday;

    [Benchmark]
    public bool IsDefined_Fast()
    {
        return DayOfWeekEnumExtensions.IsDefined(_mondayEnum);
    }

    [Benchmark]
    public bool IsDefined_Smart()
    {
        return DayOfWeekSmartEnum.TryFromValue(_mondaySmart.Value, out _);
    }

    [Benchmark]
    public string[] GetNames_Fast()
    {
        return DayOfWeekEnumExtensions.GetNames();
    }

    [Benchmark]
    public string[] GetNames_Smart()
    {
        return DayOfWeekSmartEnum.List.Select(e => e.Name).ToArray();
    }

    [Benchmark]
    public DayOfWeekEnum[] GetValues_Fast()
    {
        return DayOfWeekEnumExtensions.GetValues();
    }

    [Benchmark]
    public DayOfWeekSmartEnum[] GetValues_Smart()
    {
        return [.. DayOfWeekSmartEnum.List];
    }

    [Benchmark]
    public string Enum_ToString_Fast()
    {
        return _mondayEnum.ToStringFast();
    }

    [Benchmark]
    public string Enum_ToString_Smart()
    {
        return _mondaySmart.Name;
    }

    [Benchmark]
    public (bool, DayOfWeekEnum) TryParse_Fast()
    {
        var parsed = DayOfWeekEnumExtensions.TryParse("Monday", out var day);
        return (parsed, day);
    }

    [Benchmark]
    public (bool, DayOfWeekSmartEnum) TryParse_Smart()
    {
        var parsed = DayOfWeekSmartEnum.TryFromName("Monday", out var day);
        return (parsed, day);
    }
}
