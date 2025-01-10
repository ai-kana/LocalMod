namespace LocalMod.API.NetAbstractions;

public interface INetMethodResolver
{
    public INetMethod? ResolveClientMethod(uint id);
    public INetMethod? ResolveServerMethod(uint id);
}
