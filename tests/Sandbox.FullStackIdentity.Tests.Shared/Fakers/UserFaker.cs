using Bogus;
using Sandbox.FullStackIdentity.Domain;

namespace Sandbox.FullStackIdentity.Tests.Shared;

public class UserFaker : Faker<User>
{
    public UserFaker()
    {
        RuleFor(user => user.Id, faker => faker.Random.Guid());
        RuleFor(user => user.TenantId, faker => faker.Random.Guid());
        RuleFor(user => user.Email, faker => faker.Person.Email);
        RuleFor(user => user.UserName, (faker, user) => user.Email);
        RuleFor(user => user.FirstName, faker => faker.Person.FirstName);
        RuleFor(user => user.LastName, faker => faker.Person.LastName);
    }
}
