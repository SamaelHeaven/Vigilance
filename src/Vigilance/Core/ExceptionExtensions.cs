namespace Vigilance.Core;

public static class ExceptionExtensions
{
    extension(Exception exception)
    {
        public string DetailedString =>
            $"{exception.GetType()}: {exception.Message}{(exception.StackTrace is null ? "" : $"\n{exception.StackTrace}")}";
    }
}
