namespace Nebula.Core;

public interface ILogger
{
    ILog Log(ILog.Importance level, string text);
}

public static class LoggerHelper
{
    public static ILog Debug(this ILogger logger, string text) => logger.Log(ILog.Importance.Debug, text);
    public static ILog Message(this ILogger logger, string text) => logger.Log(ILog.Importance.Debug, text);
    public static ILog Warning(this ILogger logger, string text) => logger.Log(ILog.Importance.Debug, text);
    public static ILog Error(this ILogger logger, string text) => logger.Log(ILog.Importance.Debug, text);
}