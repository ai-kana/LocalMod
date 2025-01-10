using System.Reflection;
using HarmonyLib;
using LocalMod.API.IoC;
using LocalMod.API.NetAbstractions;
using LocalMod.API.Reflection;
using Microsoft.Extensions.DependencyInjection;
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
    private static NetMethodManager Instance = null!;

    public NetMethodManager(Harmony harmony)
    {
        Instance = this;

        Type serverMessageType = typeof(Provider).Assembly.GetNonPublicType("ServerMessageHandler_InvokeMethod")
            ?? throw new("Failed to find message server handler type");

        Type clientMessageType = typeof(Provider).Assembly.GetNonPublicType("ClientMessageHandler_InvokeMethod")
            ?? throw new("Failed to find message client handler type");

        MethodBase serverMethod = serverMessageType.GetStaticMethod("ReadMessage");
        harmony.Patch(serverMethod, new(ServerReadMessagePatch));

        MethodBase clientMethod = clientMessageType.GetStaticMethod("ReadMessage");
        harmony.Patch(clientMethod, new(ClientReadMessagePatch));

        ConvertServerMethods();
        ConvertClientMethods();

        Provider.onServerConnected += OnServerConnected;
        Provider.onServerDisconnected += OnServerDisconnected;
    }

    public void Dispose()
    {
        Provider.onServerConnected -= OnServerConnected;
        Provider.onServerDisconnected -= OnServerDisconnected;
    }

    public IReadOnlyDictionary<uint, INetMethod> ClientMethods => _ClientMethods;
    public IReadOnlyDictionary<uint, INetMethod> ServerMethods => _ServerMethods;

    private readonly Dictionary<uint, INetMethod> _ClientMethods = new();
    private readonly Dictionary<uint, INetMethod> _ServerMethods = new();

    private void RegisterFromType(Type type)
    {
        INetMethod method = (INetMethod)Activator.CreateInstance(type);
        uint id = method.NetMethodId;

        switch (method.AllowedCaller)
        {
            case NetMethodCaller.ClientCaller:
                _ClientMethods.Add(id, method);
                break;
            case NetMethodCaller.ServerCaller:
                _ServerMethods.Add(id, method);
                break;
        }
    }

    private void RegisterFromInstance(INetMethod method)
    {
        uint id = method.NetMethodId;
        switch (method.AllowedCaller)
        {
            case NetMethodCaller.ClientCaller:
                _ClientMethods.Add(id, method);
                break;
            case NetMethodCaller.ServerCaller:
                _ServerMethods.Add(id, method);
                break;
        }
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
    private static void OnServerConnected(CSteamID steamID)
    {
        RateLimits.Add(steamID, new());
    }

    private static void OnServerDisconnected(CSteamID steamID)
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

            Console.WriteLine($"Registered client RPC index: {i}, {method}");
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

            Console.WriteLine($"Registered server RPC index: {i}, {method}");
        }
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

    public void RegisterFromAssembly(Assembly assembly)
    {
        IEnumerable<Type> types = assembly.GetTypes().Where(x => x.GetInterfaces().Contains(typeof(INetMethod)));
        foreach (Type type in types)
        {
            RegisterFromType(type);
        }
    }
}
