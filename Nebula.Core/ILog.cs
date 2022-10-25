namespace Nebula.Core;

public interface ILog
{
    public enum Importance
    {
        Debug,
        Message,
        Warning,
        Error
    }
    
    string Text { get; set; }
    
    Importance Level { get; set; }
}

public static class LogHelper
{
    public static ILog SetText(this ILog log, string text)
    {
        log.Text = text;
        return log;
    }
    
    public static ILog SetLevel(this ILog log, ILog.Importance level)
    {
        log.Level = level;
        return log;
    }
}