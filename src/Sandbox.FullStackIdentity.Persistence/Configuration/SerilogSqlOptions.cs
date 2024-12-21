using Serilog.Events;
using Serilog.Sinks.PostgreSQL.ColumnWriters;

namespace Sandbox.FullStackIdentity.Persistence;

public sealed class SerilogSqlOptions
{
    public const string Key = "SerilogSQL";


    public string ConnectionString { get; set; }
    public string TableName { get; set; }
    public string SchemaName { get; set; } 

    public bool NeedAutoCreateTable { get; set; }
    public bool NeedAutoCreateSchema { get; set; }

    public Dictionary<string, DefaultColumnWriter> LoggerColumnOptions { get; set; }
    public Dictionary<string, SinglePropertyColumnWriter> LoggerPropertyColumnOptions { get; set; }

    public LogEventLevel RestrictedToMinimumLevel { get; set; }


    public SerilogSqlOptions(string connectionString)
    {
        ConnectionString = connectionString;

        TableName = "logs";
        SchemaName = string.Empty;
        LoggerColumnOptions = [];
        LoggerPropertyColumnOptions = [];
        RestrictedToMinimumLevel = LogEventLevel.Information;
    }
}
