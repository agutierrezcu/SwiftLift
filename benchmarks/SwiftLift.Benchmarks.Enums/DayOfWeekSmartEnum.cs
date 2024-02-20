using Ardalis.SmartEnum;

namespace SwiftLift.Benchmarks.Enums;

public sealed class DayOfWeekSmartEnum : SmartEnum<DayOfWeekSmartEnum>
{
    public static readonly DayOfWeekSmartEnum Sunday = new(nameof(Sunday), 1);
    public static readonly DayOfWeekSmartEnum Monday = new(nameof(Monday), 2);
    public static readonly DayOfWeekSmartEnum Tuesday = new(nameof(Tuesday), 3);
    public static readonly DayOfWeekSmartEnum Wednesday = new(nameof(Wednesday), 4);
    public static readonly DayOfWeekSmartEnum Thursday = new(nameof(Thursday), 5);
    public static readonly DayOfWeekSmartEnum Friday = new(nameof(Friday), 6);
    public static readonly DayOfWeekSmartEnum Saturday = new(nameof(Saturday), 7);

    private DayOfWeekSmartEnum(string name, int value) : base(name, value)
    {
    }
}
