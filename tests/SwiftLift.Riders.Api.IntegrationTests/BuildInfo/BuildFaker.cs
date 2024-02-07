using System.Globalization;
using Bogus;
using SwiftLift.Infrastructure.BuildInfo;

namespace SwiftLift.Riders.Api.IntegrationTests.BuildInfo;

public class BuildFaker : AutoFaker<Build>
{
    public static readonly BuildFaker Instance = new();

    private BuildFaker()
    {
        RuleFor(fake => fake.Id, PickRandomInt);
        RuleFor(fake => fake.Number, PickRandomInt);
        RuleFor(fake => fake.Commit, faker => faker.Random.Uuid().ToString());

        static string PickRandomInt(Faker faker)
        {
            return faker.Random.Int(1, 99999).ToString(CultureInfo.InvariantCulture);
        }
    }
}

