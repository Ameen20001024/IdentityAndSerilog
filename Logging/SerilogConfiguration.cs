using Serilog;
using Serilog.Events;

namespace IdentityAndSerilog.Logging;

public static class SerilogConfiguration
{
    public static LoggerConfiguration ConfigureApplicationLogging(
        this LoggerConfiguration loggerConfiguration)
    {
        return loggerConfiguration
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            
            // Application logs
            .WriteTo.Logger(log => log
                .Filter.ByExcluding(Serilog.Filters.Matching.WithProperty<int>("EventId.Id", id => id >= 4000 && id < 5000))
                .WriteTo.File(
                    path: "Logs/Application/application-.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    shared: true,
                    outputTemplate:
                        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} " +
                        "[{Level:u3}] " +
                        "[{SourceContext}] " +
                        "{Message:lj}{NewLine}{Exception}"))

            // Security logs
            .WriteTo.Logger(log => log
                .Filter.ByIncludingOnly(Serilog.Filters.Matching.WithProperty<int>("EventId.Id", id => id >= 4000 && id < 5000))
                .WriteTo.File(
                    path: "Logs/Security/security-.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 90,
                    shared: true,
                    outputTemplate:
                        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} " +
                        "[{Level:u3}] " +
                        "[{SourceContext}] " +
                        "{Message:lj}{NewLine}{Exception}"))

            

            // Console
            .WriteTo.Console(
                outputTemplate:
                    "{Timestamp:HH:mm:ss} " +
                    "[{Level:u3}] " +
                    "[{SourceContext}] " +
                    "{Message:lj}{NewLine}{Exception}");
    }

    private static Func<LogEvent, bool> IsCategory(string category)
    {
        return logEvent =>
            logEvent.Properties.TryGetValue(
                LogProperties.Category,
                out var value)
            && value is ScalarValue { Value: string valueString }
            && valueString == category;
    }
}