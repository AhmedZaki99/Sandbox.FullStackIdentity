using FluentResults;

namespace Sandbox.FullStackIdentity.Domain;

public interface ISubscriptionRepository
{
    Task<bool?> IsSubscribedAsync(string email, SubscriptionScope scope, CancellationToken cancellationToken = default);
    Task<string[]> GetSubscribedEmailsAsync(SubscriptionScope scope, CancellationToken cancellationToken = default);
    Task<string[]> GetUnsubscribedEmailsAsync(SubscriptionScope scope, CancellationToken cancellationToken = default);

    Task<Result> SubscribeAsync(string email, SubscriptionScope[] scopes, CancellationToken cancellationToken = default);
    Task<Result> SubscribeAsync(string email, SubscriptionScope scope, CancellationToken cancellationToken = default);
    
    Task<Result> UnsubscribeAsync(string email, SubscriptionScope[] scopes, CancellationToken cancellationToken = default);
    Task<Result> UnsubscribeAsync(string email, SubscriptionScope scope, CancellationToken cancellationToken = default);    

    Task<Result> UpdateSubscriptionsAsync(string email, Dictionary<SubscriptionScope, bool> scopeDictionary, CancellationToken cancellationToken = default);
    Task<Result> ResetSubscriptionsAsync(string email, CancellationToken cancellationToken = default);

    Task<Result> SubscribeToAllAsync(string email, CancellationToken cancellationToken = default);
    Task<Result> UnsubscribeToAllAsync(string email, CancellationToken cancellationToken = default);
}
