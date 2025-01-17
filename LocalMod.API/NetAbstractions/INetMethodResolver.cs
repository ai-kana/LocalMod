namespace LocalMod.API.NetAbstractions;

public interface INetMethodResolver
{
    public INetMethod? ResolveServerMethod(Type type);
    public INetMethod? ResolveClientMethod(Type type);
}

public static class INetMethodResolverExtensions
{
    public static INetMethod? ResolveServerMethod<T>(this INetMethodResolver resolver)
    {
        return resolver.ResolveServerMethod(typeof(T));
    }

    public static INetMethod? ResolveClientMethod<T>(this INetMethodResolver resolver)
    {
        return resolver.ResolveClientMethod(typeof(T));
    }
}
