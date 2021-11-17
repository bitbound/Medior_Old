namespace Medior.Extensions
{
    public static class GenericExtensions
    {
        public static T Apply<T>(this T self, Action<T> action)
        {
            action(self);
            return self;
        }
    }
}
