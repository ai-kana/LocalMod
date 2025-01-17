using LocalMod.API.IoC;
using LocalMod.API.NetAbstractions;
using Microsoft.Extensions.DependencyInjection;

namespace LocalMod.Core.NetAbstractions;

[Service(ServiceLifetime.Singleton, typeof(INetMethodResolver))]
internal class NetMethodResolver : INetMethodResolver
{
    private readonly NetMethodManager _Manager;
    public NetMethodResolver(NetMethodManager manager)
    {
        _Manager = manager;
    }

    public INetMethod? ResolveServerMethod(Type type)
    {
        return _Manager.ServerMethods.Values.FirstOrDefault(x => x.GetType() == type);
    }

    public INetMethod? ResolveClientMethod(Type type)
    {
        return _Manager.ClientMethods.Values.FirstOrDefault(x => x.GetType() == type);
    }
}
