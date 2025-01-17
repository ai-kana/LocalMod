using System.Reflection;
using Cysharp.Threading.Tasks;
using HarmonyLib;
using LocalMod.API.IoC;
using LocalMod.API.NetAbstractions;
using LocalMod.API.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SDG.NetPak;
using SDG.NetTransport;
using SDG.Unturned;
using Steamworks;
using UnityEngine;

namespace LocalMod.Core.NetAbstractions;

[HarmonyPatch]
[Service(ServiceLifetime.Singleton)]
internal class NetMethodManager : IDisposable
{
    // For patches to access
    public static NetMethodManager Instance {get; private set;} = null!;
    private readonly ILogger _Logger;

    public NetMethodManager(ILogger<NetMethodManager> logger, Harmony harmony)
    {
        Instance = this;
        _Logger = logger;

        Type serverMessageType = typeof(Provider).Assembly.GetNonPublicType("ServerMessageHandler_InvokeMethod")
            ?? throw new("Failed to find message server handler type");

        Type clientMessageType = typeof(Provider).Assembly.GetNonPublicType("ClientMessageHandler_InvokeMethod")
            ?? throw new("Failed to find message client handler type");

        MethodBase serverMethod = serverMessageType.GetStaticMethod("ReadMessage");
        harmony.Patch(serverMethod, new(ServerReadMessagePatch));

        MethodBase clientMethod = clientMessageType.GetStaticMethod("ReadMessage");
        harmony.Patch(clientMethod, new(ClientReadMessagePatch));

        CreateDefaultRPCs();

        Provider.onServerHosted += OnServerHosted;

        _AvailableMethods.Add(new TestRPC());
    }

    private class TestRPC : ServerNetMethod
    {
        public override void ReceiveInvoke(ClientInvocationData data)
        {
            throw new NotImplementedException();
        }

        public override void SendInvoke(NetPakWriter writer)
        {
            throw new NotImplementedException();
        }
    }

    public void ReceiveSyncRPC(SyncData[] rpcs)
    {
        _Logger.LogInformation("Received sync rpc");
        CreateDefaultRPCs();

        ClientNetMethod<bool, string?> confirm = new ConfirmLoadedRPC();
        foreach (SyncData rpc in rpcs)
        {
            INetMethod method = _AvailableMethods.FirstOrDefault(x => x.GetType().FullName == rpc.RpcTypeName);
            if (method == default)
            {
                confirm.Invoke(true, rpc.RpcTypeName);
                return;
            }

            method.NetMethodId = rpc.RpcId;

            switch (method.AllowedCaller)
            {
                case NetMethodCaller.ServerCaller:
                    _ServerMethods.Add(method.NetMethodId, method);
                    continue;
                case NetMethodCaller.ClientCaller:
                    _ClientMethods.Add(method.NetMethodId, method);
                    continue;
            }
        }

        confirm.Invoke(false, null);
    }

    private void OnServerHosted()
    {
        _Logger.LogInformation("Hosted");
        Provider.onServerConnected += OnServerConnected;
        Provider.onServerDisconnected += OnServerDisconnected;

        // make room for the statically defined sync rpc and failed sync rpc
        uint serverId = (uint)InternalClientMethods.Count + 1;
        uint clientId = (uint)InternalServerMethods.Count + 1;
        foreach (INetMethod method in _AvailableMethods)
        {
            switch (method.AllowedCaller)
            {
                case NetMethodCaller.ServerCaller:
                    serverId++;
                    method.NetMethodId = serverId;
                    _ServerMethods.Add(serverId, method);
                    continue;
                case NetMethodCaller.ClientCaller:
                    clientId++;
                    method.NetMethodId = clientId;
                    _ClientMethods.Add(clientId, method);
                    continue;
            }
        }
    }

    public void Dispose()
    {
        if (Provider.isServer)
        {
            Provider.onServerConnected -= OnServerConnected;
            Provider.onServerDisconnected -= OnServerDisconnected;
        }
    }

    // Methods that can be called by the server
    public IReadOnlyDictionary<uint, INetMethod> ServerMethods => _ServerMethods;
    // Methods that can be called by the client
    public IReadOnlyDictionary<uint, INetMethod> ClientMethods => _ClientMethods;

    private readonly Dictionary<uint, INetMethod> _ServerMethods = new();
    private readonly Dictionary<uint, INetMethod> _ClientMethods = new();

    public IReadOnlyList<INetMethod> AvailableMethods => _AvailableMethods;
    private readonly List<INetMethod> _AvailableMethods = new();

    private void RegisterFromType(Type type)
    {
        INetMethod method = (INetMethod)Activator.CreateInstance(type);
        RegisterFromInstance(method);
    }

    private void RegisterFromInstance(INetMethod method)
    {
        uint id = method.NetMethodId;
        _Logger.LogDebug($"Registered RPC: {method.ToString()}");
        _AvailableMethods.Add(method);
    }

    private readonly struct RateLimitData
    {
        public readonly float LastCall;
        public readonly float LimitedHits;
        public RateLimitData(float lastCall, float limitedHits)
        {
            LastCall = lastCall;
            LimitedHits = limitedHits;
        }
    }

    private static Dictionary<CSteamID, Dictionary<uint, RateLimitData>> RateLimits = new();
    private void OnServerConnected(CSteamID steamID)
    {
        RateLimits.Add(steamID, new());

        int count = _AvailableMethods.Count;
        SyncData[] rpcs = new SyncData[count];
        for (int i = 0; i < count; i++)
        {
            INetMethod method = _AvailableMethods[i];
            rpcs[i] = new(method);
        }

        ServerNetMethod<SyncData[]> syncRpc = new SyncRPC();
        ITransportConnection connection = Provider.findTransportConnection(steamID);
        syncRpc.Invoke(rpcs, connection);
    }

    private void OnServerDisconnected(CSteamID steamID)
    {
        RateLimits.Remove(steamID);
    }

    private static bool IsRateLimited(INetMethod method, SteamPlayer caller)
    {
        if (method.RateLimit >= 0)
        {
            return false;
        }

        Dictionary<uint, RateLimitData> rateLimits = RateLimits[caller.playerID.steamID];
        float time = Time.realtimeSinceStartup;

        if (!rateLimits.TryGetValue(method.NetMethodId, out RateLimitData data))
        {
            rateLimits.Add(method.NetMethodId, new(time, 0));
            return false;
        }

        if (time >= data.LastCall)
        {
            rateLimits[method.NetMethodId] = new(time + method.RateLimit, 0);
            return false;
        }

        rateLimits[method.NetMethodId] = new(time, data.LimitedHits + 1);
        int maxHits = Mathf.Max(2, Provider.configData.Server.Rate_Limit_Kick_Threshold);
        if (rateLimits[method.NetMethodId].LimitedHits >= maxHits)
        {
            Provider.kick(caller.playerID.steamID, $"significantly exceeded rate limit ({maxHits} times in {method.RateLimit} seconds)");
        }

        return true;
    }

    private static bool ServerReadMessagePatch(ITransportConnection transportConnection, NetPakReader reader)
    {
        if (!reader.ReadUInt32(out uint index))
        {
            Provider.refuseGarbageConnection(transportConnection, "unable to read method id");
            return false;
        }

        if (!Instance._ClientMethods.TryGetValue(index, out INetMethod method))
        {
            Provider.refuseGarbageConnection(transportConnection, "invalid method id");
            return false;
        }
        Console.WriteLine($"Read id: {index}, {method}");

        SteamPlayer caller = Provider.findPlayer(transportConnection);
        if (IsRateLimited(method, caller))
        {
            return false;
        }

        try
        {
            caller.timeLastPacketWasReceivedFromClient = Time.realtimeSinceStartup;
            method.ReceiveInvoke(new(caller, reader));
        }
        catch (Exception e)
        {
            UnturnedLog.exception(e, $"Exception invoking rpc from client id: {method.NetMethodId}");
        }

        return false;
    }

    private static bool ClientReadMessagePatch(NetPakReader reader)
    {
        if (!reader.ReadUInt32(out uint index))
        {
            return false;
        }

        if (!Instance._ServerMethods.TryGetValue(index, out INetMethod method))
        {
            return false;
        }

        InvocationData data = new(reader);
        try
        {
            method.ReceiveInvoke(data);
        }
        catch (Exception e)
        {
            UnturnedLog.exception(e, $"Exception invoking {method.NetMethodId} from server:");
        }

        return false;
    }

    public readonly static List<ClientMethodInfo> InternalClientMethods = 
        typeof(NetReflection)
        .GetStaticField("clientMethods")
        .GetValue<List<ClientMethodInfo>>()
        ?? throw new("Failed to find ClientMethods field");

    public readonly static List<ServerMethodInfo> InternalServerMethods = 
        typeof(NetReflection)
        .GetStaticField("serverMethods")
        .GetValue<List<ServerMethodInfo>>()
        ?? throw new("Failed to find ServerMethods field");

    private void ConvertClientMethods()
    {
        int count = InternalClientMethods.Count;
        for (int i = 0; i < count; i++)
        {
            ClientMethodInfo method = InternalClientMethods[i];
            InternalClientMethod newMethod = new((uint)i, method);
            _ServerMethods.Add((uint)i, newMethod);
        }
    }

    private void ConvertServerMethods()
    {
        int count = InternalServerMethods.Count;
        for (int i = 0; i < count; i++)
        {
            ServerMethodInfo method = InternalServerMethods[i];
            InternalServerNetMethod newMethod = new((uint)i, method);
            _ClientMethods.Add((uint)i, newMethod);
        }
    }

    private void CreateDefaultRPCs()
    {
        _ServerMethods.Clear();
        _ClientMethods.Clear();

        ConvertClientMethods();
        ConvertServerMethods();

        SyncRPC sync = new();
        _ServerMethods.Add(sync.NetMethodId, sync);

        ConfirmLoadedRPC confirm = new();
        _ClientMethods.Add(confirm.NetMethodId, confirm);
    }

    private readonly static NetPakWriter Writer =
        typeof(Provider).Assembly.GetNonPublicType("NetMessages")
        .GetStaticField("writer")
        .GetValue<NetPakWriter>()
        ?? throw new("Failed to get netpak writer from net messages");

    private readonly static FieldInfo ServerMethodInfoField =
        typeof(ServerMethodHandle).GetInstanceField("serverMethodInfo")
        ?? throw new("Failed to get server method info field");

    [HarmonyPatch(typeof(ServerMethodHandle), "GetWriterWithStaticHeader")]
    [HarmonyPrefix]
    private static bool GetServerWriterPatch(ServerMethodHandle __instance, ref NetPakWriter __result)
    {
        NetPakWriter writer = Writer;
        writer.Reset();
        writer.WriteEnum(EServerMessage.InvokeMethod);

        ServerMethodInfo info = (ServerMethodInfo)ServerMethodInfoField.GetValue(__instance);
        uint id = (uint)ServerMethodInfoFields.MethodIndexField.GetValue(info);
        writer.WriteUInt32(id);
        __result = writer;

        return false;
    }

    private readonly static FieldInfo ClientMethodInfoField =
        typeof(ClientMethodHandle).GetInstanceField("clientMethodInfo")
        ?? throw new("Failed to get server method info field");

    [HarmonyPatch(typeof(ClientMethodHandle), "GetWriterWithStaticHeader")]
    [HarmonyPrefix]
    private static bool GetClientWriterPatch(ClientMethodHandle __instance, ref NetPakWriter __result)
    {
        NetPakWriter writer = Writer;
        writer.Reset();
        writer.WriteEnum(EClientMessage.InvokeMethod);

        ClientMethodInfo info = (ClientMethodInfo)ClientMethodInfoField.GetValue(__instance);
        uint id = (uint)ClientMethodInfoFields.MethodIndexField.GetValue(info);
        writer.WriteUInt32(id);
        __result = writer;

        return false;
    }

    private readonly static NetPakReader Reader = typeof(Provider).Assembly
        .GetNonPublicType("NetMessages")
        .GetStaticField("reader")
        .GetValue<NetPakReader>()
        ?? throw new("Failed to get reader");

    private readonly static byte[] ProviderBuffer =
        typeof(Provider)
        .GetStaticField("buffer")
        .GetValue<byte[]>()
        ?? throw new("Failed to get provider buffer");

    [HarmonyPatch(typeof(ClientMethodHandle), "InvokeLoopback")]
    [HarmonyPrefix]
    private static bool ClientInvokeLoopbackPatch(ClientMethodHandle __instance, NetPakWriter writer)
    {
        NetPakReader reader = Reader;
        reader.SetBufferSegmentCopy(writer.buffer, ProviderBuffer, writer.writeByteIndex);
        reader.Reset();
        reader.ReadBits(5, out _);
        reader.ReadUInt32(out uint index);

        if (!Instance._ServerMethods.TryGetValue(index, out INetMethod method))
        {
            return false;
        }

        InvocationData data = new(reader);
        try
        {
            method.ReceiveInvoke(data);
        }
        catch (Exception exception)
        {
            UnturnedLog.exception(exception, "Failed to invoke loopback");
        }

        return false;
    }

    [HarmonyPatch(typeof(ServerMethodHandle), "InvokeLoopback")]
    [HarmonyPrefix]
    private static bool ServerInvokeLoopbackPatch(ServerMethodHandle __instance, NetPakWriter writer)
    {
        NetPakReader reader = Reader;
        reader.SetBufferSegmentCopy(writer.buffer, ProviderBuffer, writer.writeByteIndex);
        reader.Reset();
        reader.ReadBits(4, out _);
        reader.ReadUInt32(out uint index);

        if (!Instance._ClientMethods.TryGetValue(index, out INetMethod method))
        {
            return false;
        }

        InvocationData data = new(Provider.clients[0], reader);
        try
        {
            method.ReceiveInvoke(data);
        }
        catch (Exception exception)
        {
            UnturnedLog.exception(exception, "Failed to invoke loopback");
        }

        return false;
    }

    public void RegisterFromAssembly(Assembly assembly)
    {
        IEnumerable<Type> types = assembly.GetTypes().Where(x => x.GetInterfaces().Contains(typeof(INetMethod)));
        foreach (Type type in types)
        {
            RegisterFromType(type);
        }
    }
}
