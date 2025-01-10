using LocalMod.API.IoC;
using LocalMod.API.NetAbstractions;
using Microsoft.Extensions.DependencyInjection;

namespace LocalMod.Core.NetAbstractions;

[Service(ServiceLifetime.Singleton, typeof(INetMethodResolver))]
public class NetInvokableResolver : INetMethodResolver
{
    private readonly NetMethodManager _Manager;
    internal NetInvokableResolver(NetMethodManager manager)
    {
        _Manager = manager;
    }

    public INetMethod? ResolveClientMethod(uint id)
    {
        _Manager.ClientMethods.TryGetValue(id, out INetMethod method);
        return method;
    }

    public INetMethod? ResolveServerMethod(uint id)
    {
        _Manager.ServerMethods.TryGetValue(id, out INetMethod method);
        return method;
    }
}
