using Serilog;

namespace IdentityAndSerilog.Logging;

public static class LoggerExtensions
{
    public static Serilog.ILogger ForApplication(this Serilog.ILogger logger)
    {
        return logger.ForContext(LogProperties.Category, LogCategory.Application);
    }

    public static Serilog.ILogger ForSecurity(this Serilog.ILogger logger)
    {
        return logger.ForContext(LogProperties.Category, LogCategory.Security);
    }

    public static Serilog.ILogger ForSystem(this Serilog.ILogger logger)
    {
        return logger.ForContext(LogProperties.Category, LogCategory.System);
    }

    //public static Serilog.ILogger ForNetwork(this Serilog.ILogger logger)
    //{
    //    return logger.ForContext(LogProperties.Category, LogCategory.Network);
    //}
}