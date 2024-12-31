using System.Data;
using Dapper;
using Npgsql;
using NpgsqlTypes;

namespace Sandbox.FullStackIdentity.Persistence;

public class HstoreTypeHandler<TKey, TValue> : SqlMapper.TypeHandler<Dictionary<TKey, TValue>>
    where TKey : notnull
{
    private readonly Func<string, TKey> _keyParser;
    private readonly Func<string, TValue> _valueParser;

    public HstoreTypeHandler(Func<string, TKey> keyParser, Func<string, TValue> valueParser)
    {
        _keyParser = keyParser;
        _valueParser = valueParser;
    }


    public override Dictionary<TKey, TValue>? Parse(object value)
    {
        if (value is null)
        {
            return null;
        }
        if (value is not Dictionary<string, string> hstore)
        {
            throw new ArgumentException($"Unable to convert {value.GetType()} to Dictionary<{typeof(TKey)}, {typeof(TValue)}>");
        }
        return hstore.Select(ConvertFromStringPair).ToDictionary();
    }

    public override void SetValue(IDbDataParameter parameter, Dictionary<TKey, TValue>? value)
    {
        if (value is null)
        {
            parameter.Value = DBNull.Value;
            return;
        }
        if (parameter is not NpgsqlParameter npgsqlParameter)
        {
            throw new ArgumentException($"Parameter must be of type NpgsqlParameter, not {parameter.GetType()}");
        }
        npgsqlParameter.NpgsqlDbType = NpgsqlDbType.Hstore;
        npgsqlParameter.Value = value.Select(ConvertToStringPair).ToDictionary();
    }


    private KeyValuePair<TKey, TValue> ConvertFromStringPair(KeyValuePair<string, string> kvp)
    {
        try
        {
            return new(_keyParser(kvp.Key), _valueParser(kvp.Value));
        }
        catch (Exception ex) when (ex is FormatException or InvalidCastException)
        {
            throw new InvalidCastException(
                $"Could not convert hstore value. Key: '{kvp.Key}', Value: '{kvp.Value}' to types <{typeof(TKey)}, {typeof(TValue)}>", ex);
        }
    }

    private KeyValuePair<string, string> ConvertToStringPair(KeyValuePair<TKey, TValue> kvp)
    {
        return new(
            kvp.Key?.ToString() ?? throw new ArgumentException("Key cannot be null"), 
            kvp.Value?.ToString() ?? string.Empty);
    }
}
