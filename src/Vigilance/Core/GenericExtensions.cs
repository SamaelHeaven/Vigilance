namespace Vigilance.Core;

public static class GenericExtensions
{
    extension<T>(T t)
    {
        public T Out(out T value)
        {
            value = t;
            return t;
        }

        public T Tap(Action<T> action)
        {
            action.Invoke(t);
            return t;
        }

        public T Tap(Func<T, T> func)
        {
            return func.Invoke(t);
        }
    }
}
