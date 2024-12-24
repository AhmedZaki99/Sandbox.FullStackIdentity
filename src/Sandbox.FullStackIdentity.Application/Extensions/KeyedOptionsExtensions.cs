using System.Linq.Expressions;
using Sandbox.FullStackIdentity.Application;

namespace Sandbox.FullStackIdentity.DependencyInjection;

public static class KeyedOptionsExtensions
{
    /// <summary>
    /// Requires the specified property to be set by throwing an <see cref="InvalidOperationException"/> if it is not.
    /// </summary>
    /// <param name="options">The keyed options.</param>
    /// <param name="propertyExpression">The property expression to check.</param>
    /// <returns>The keyed options to allow chaining property checks.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static TOptions RequireProperty<TOptions>(this TOptions options, Expression<Func<TOptions, string>> propertyExpression) 
        where TOptions : IKeyedOptions
    {
        if (string.IsNullOrWhiteSpace(propertyExpression.Compile().Invoke(options)))
        {
            var propertyName = (propertyExpression.Body as MemberExpression)?.Member.Name
                ?? throw new ArgumentException("Invalid property expression.", nameof(propertyExpression));

            throw new InvalidOperationException($"{options.Key} configuration property '{propertyName}' is required.");
        }
        return options;
    }

    /// <summary>
    /// Requires the specified property to be set by throwing an <see cref="InvalidOperationException"/> if it is not.
    /// </summary>
    /// <param name="options">The keyed options.</param>
    /// <param name="propertyExpression">The property expression to check.</param>
    /// <returns>The keyed options to allow chaining property checks.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static TOptions RequireProperty<TOptions, TProperty>(this TOptions options, Expression<Func<TOptions, TProperty>> propertyExpression)
        where TOptions : IKeyedOptions
        where TProperty : struct
    {
        if (propertyExpression.Compile().Invoke(options).Equals(default(TProperty)))
        {
            var propertyName = (propertyExpression.Body as MemberExpression)?.Member.Name
                ?? throw new ArgumentException("Invalid property expression.", nameof(propertyExpression));

            throw new InvalidOperationException($"{options.Key} configuration property '{propertyName}' is required.");
        }
        return options;
    }
}
