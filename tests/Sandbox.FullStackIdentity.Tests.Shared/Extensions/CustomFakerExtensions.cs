using Bogus;

namespace Sandbox.FullStackIdentity.Tests.Shared;

public static class CustomFakerExtensions
{
    public static Faker<T> UsePrivateConstructor<T>(this Faker<T> faker) where T : class
    {
        return faker.CustomInstantiator(f => Activator.CreateInstance(typeof(T), nonPublic: true) as T 
            ?? throw new InvalidOperationException("Unable to instantiate a fake object using its non-public constructor."));
    }
}
