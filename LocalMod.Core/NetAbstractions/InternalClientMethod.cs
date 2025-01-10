using LocalMod.API.NetAbstractions;
using LocalMod.API.Reflection;
using SDG.Unturned;

namespace LocalMod.Core.NetAbstractions;

internal class InternalClientMethod : INetMethod
{
    public InternalClientMethod(uint id, ClientMethodInfo info)
    {
        _MethodInfo = info;
        SteamCall call = (SteamCall)ClientMethodInfoFields.CustomAttributeField.GetValue(_MethodInfo);
        _ReadMethod = (ClientMethodReceive)ClientMethodInfoFields.ReadMethodField.GetValue(_MethodInfo);
        _RateLimit = call.ratelimitSeconds;
        _NetMethodId = id;
    }

    public NetMethodCaller AllowedCaller => NetMethodCaller.ServerCaller;

    private readonly ClientMethodInfo _MethodInfo;
    private readonly ClientMethodReceive _ReadMethod;

    private readonly float _RateLimit;
    public float RateLimit => _RateLimit;

    private readonly uint _NetMethodId;
    public uint NetMethodId => _NetMethodId;

    public void ReceiveInvoke(InvocationData data)
    {
        object[] args = [];
        ClientInvocationContext context = ActivatorEx.CreateInstance<ClientInvocationContext>(
                ClientInvocationContext.EOrigin.Remote, 
                data.Reader, 
                _MethodInfo);

        _ReadMethod(context);
    }
}
