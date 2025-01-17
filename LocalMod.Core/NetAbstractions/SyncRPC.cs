using LocalMod.API.NetAbstractions;
using SDG.NetPak;
using SDG.Unturned;

namespace LocalMod.Core.NetAbstractions;

internal readonly struct SyncData
{
    public readonly string RpcTypeName;
    public readonly uint RpcId;

    public SyncData(Type rpcType, uint rpcId)
    {
        RpcTypeName = rpcType.FullName;
        RpcId = rpcId;
    }

    public SyncData(INetMethod method)
    {
        RpcTypeName = method.GetType().FullName;
        RpcId = method.NetMethodId;
    }

    public SyncData(string rpcTypeName, uint rpcId)
    {
        RpcTypeName = rpcTypeName;
        RpcId = rpcId;
    }
}

internal class SyncRPC : ServerNetMethod<SyncData[]>
{
    private uint _NetMethodId = (uint)(NetMethodManager.InternalClientMethods.Count + 1);
    public override uint NetMethodId 
    { 
        get => _NetMethodId; 
        set => throw new NotSupportedException(); 
    }

    public override void ReceiveInvoke(ClientInvocationData data)
    {
        NetPakReader reader = data.Reader;
        reader.ReadUInt32(out uint length);

        SyncData[] rpcs = new SyncData[length];

        for (int i = 0; i < length; i++)
        {
            reader.ReadString(out string name);
            reader.ReadUInt32(out uint id);
            rpcs[i] = new(name, id);
        }
        
        NetMethodManager.Instance.ReceiveSyncRPC(rpcs);
    }

    public override void SendInvoke(NetPakWriter writer, SyncData[] rpcs)
    {
        writer.WriteInt32(rpcs.Length);

        for (int i = 0; i < rpcs.Length; i++)
        {
            writer.WriteString(rpcs[i].RpcTypeName);
            writer.WriteUInt32(rpcs[i].RpcId);
        }
    }
}

// Ask server to kick you when you fail to resolve an RPC
internal class FailedSyncRPC : ClientNetMethod<string>
{
    private uint _NetMethodId = (uint)(NetMethodManager.InternalServerMethods.Count + 1);
    public override uint NetMethodId 
    { 
        get => _NetMethodId; 
        set => throw new NotSupportedException(); 
    }

    public override void ReceiveInvoke(ServerInvocationData data)
    {
        NetPakReader reader = data.Reader;
        reader.ReadString(out string rpcName);
        Provider.kick(data.Caller.playerID.steamID, $"Failed to resolve RPC: {rpcName}");
    }

    public override void SendInvoke(NetPakWriter writer, string rpcName)
    {
        writer.WriteString(rpcName);
    }
}
