using SDG.NetPak;
using SDG.Unturned;

namespace LocalMod.API.NetAbstractions;

public readonly struct InvocationData
{
    public readonly NetPakReader Reader;
    /// <summary>
    /// is null when in ClientNetMethod
    /// </summary>
    public readonly SteamPlayer? Caller;

    public InvocationData(SteamPlayer caller, NetPakReader reader)
    {
        Caller = caller;
        Reader = reader;
    }

    public InvocationData(NetPakReader reader)
    {
        Reader = reader;
        Caller = null;
    }
}

public readonly struct ServerInvocationData
{
    public readonly NetPakReader Reader;
    public readonly SteamPlayer Caller;

    public ServerInvocationData(InvocationData data)
    {
        Caller = data.Caller!;
        Reader = data.Reader;
    }
}

public readonly struct ClientInvocationData
{
    public readonly NetPakReader Reader;
    public ClientInvocationData(InvocationData data)
    {
        Reader = data.Reader;
    }
}
