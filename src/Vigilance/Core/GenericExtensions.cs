namespace Vigilance.Core;

public static class GenericExtensions
{
    extension<T>(T t)
    {
        public T Tap(out T value)
        {
            value = t;
            return t;
        }

        public T Tap(Action<T> action)
        {
            action.Invoke(t);
            return t;
        }
    }
}
