using System.Reflection;

namespace LocalMod.API.Reflection;

public static class ActivatorEx
{
    private const BindingFlags Flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
    public static T CreateInstance<T>(params object[] args)
    {
        return (T)Activator.CreateInstance(typeof(T), Flags, null, args, null);
    }
}
