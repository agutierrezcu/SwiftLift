using System.Globalization;
using Bogus;
using SwiftLift.Infrastructure.Build;

namespace SwiftLift.Riders.Api.IntegrationTests;

public class BuildInfoFaker : AutoFaker<BuildInfo>
{
    public static readonly BuildInfoFaker Instance = new();

    private BuildInfoFaker()
    {
        base.RuleFor(fake => fake.Id, PickRandomInt);
        RuleFor(fake => fake.Number, PickRandomInt);
        RuleFor(fake => fake.Commit, faker => faker.Random.Uuid().ToString());

        static string PickRandomInt(Faker faker)
        {
            return faker.Random.Int(1, 99999).ToString(CultureInfo.InvariantCulture);
        }
    }
}

