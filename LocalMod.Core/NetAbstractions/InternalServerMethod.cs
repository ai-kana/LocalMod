using LocalMod.API.NetAbstractions;
using LocalMod.API.Reflection;
using SDG.Unturned;

namespace LocalMod.Core.NetAbstractions;

internal class InternalServerNetMethod : INetMethod
{
    public InternalServerNetMethod(uint id, ServerMethodInfo info)
    {
        _MethodInfo = info;
        SteamCall call = (SteamCall)ServerMethodInfoFields.CustomAttributeField.GetValue(_MethodInfo);
        _ReadMethod = (ServerMethodReceive)ServerMethodInfoFields.ReadMethodField.GetValue(_MethodInfo);
        _RateLimit = call.ratelimitSeconds;
        _NetMethodId = id;
    }

    public override string ToString()
    {
        return _MethodInfo.ToString();
    }

    public NetMethodCaller AllowedCaller => NetMethodCaller.ClientCaller;

    private readonly ServerMethodInfo _MethodInfo;
    private readonly ServerMethodReceive _ReadMethod;

    private readonly float _RateLimit;
    public float RateLimit => _RateLimit;

    private readonly uint _NetMethodId;
    public uint NetMethodId 
    {
        get => _NetMethodId;
        set => throw new NotImplementedException();
    }

    public void ReceiveInvoke(InvocationData data)
    {
        ServerInvocationContext context = ActivatorEx.CreateInstance<ServerInvocationContext>(
                ServerInvocationContext.EOrigin.Remote, 
                data.Caller!, 
                data.Reader, 
                _MethodInfo);

        _ReadMethod(context);
    }
}
