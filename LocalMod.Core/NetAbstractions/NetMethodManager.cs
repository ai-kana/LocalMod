using System.Reflection;
using LocalMod.Core.Logging;
using Microsoft.Extensions.Logging;
using SDG.NetPak;
using SDG.Unturned;

namespace LocalMod.Core.NetAbstractions;

public class NetMethodManager
{
    public static void DumpRPCs()
    {
        ILogger logger = Logger.CreateLogger<NetMethodManager>();
        using (logger.BeginScope("RPCDump"))
        {
            int index = 0;
            using (logger.BeginScope("Client"))
            {
                foreach (ClientMethodInfo info in ClientMethods)
                {
                    logger.LogInformation($"{index} :: {info.ToString()}");
                    index++;
                }

                logger.LogInformation($"Found {index} RPCs");
            }

            index = 0;
            using (logger.BeginScope("Server"))
            {
                foreach (ServerMethodInfo info in ServerMethods)
                {
                    logger.LogInformation($"{index} :: {info.ToString()}");
                    index++;
                }

                logger.LogInformation($"Found {index} RPCs");
            }
        }
    }

    private static class ServerMethodInfoFields
    {
        public readonly static FieldInfo DeclaringTypeField = typeof(ServerMethodInfo).GetField("declaringType", BindingFlags.Instance | BindingFlags.NonPublic);
        public readonly static FieldInfo NameField = typeof(ServerMethodInfo).GetField("name", BindingFlags.Instance | BindingFlags.NonPublic);
        public readonly static FieldInfo DebugNameField = typeof(ServerMethodInfo).GetField("debugName", BindingFlags.Instance | BindingFlags.NonPublic);
        public readonly static FieldInfo CustomAttributeField = typeof(ServerMethodInfo).GetField("customAttribute", BindingFlags.Instance | BindingFlags.NonPublic);
        public readonly static FieldInfo ReadMethodField = typeof(ServerMethodInfo).GetField("readMethod", BindingFlags.Instance | BindingFlags.NonPublic);
        public readonly static FieldInfo WriteMethodInfoField = typeof(ServerMethodInfo).GetField("writeMethodInfo", BindingFlags.Instance | BindingFlags.NonPublic);
        public readonly static FieldInfo MethodIndexField = typeof(ServerMethodInfo).GetField("methodIndex", BindingFlags.Instance | BindingFlags.NonPublic);
        public readonly static FieldInfo RateLimitIndexField = typeof(ServerMethodInfo).GetField("rateLimitIndex", BindingFlags.Instance | BindingFlags.NonPublic);
        public readonly static FieldInfo RateLimitedMethodsCountField = typeof(ServerMethodInfo).GetField("rateLimitedMethodsCount", BindingFlags.Instance | BindingFlags.NonPublic);
    }

    private static class ClientMethodInfoFields
    {
        public readonly static FieldInfo DeclaringTypeField = typeof(ClientMethodInfo).GetField("declaringType", BindingFlags.Instance | BindingFlags.NonPublic);
        public readonly static FieldInfo NameField = typeof(ClientMethodInfo).GetField("name", BindingFlags.Instance | BindingFlags.NonPublic);
        public readonly static FieldInfo DebugNameField = typeof(ClientMethodInfo).GetField("debugName", BindingFlags.Instance | BindingFlags.NonPublic);
        public readonly static FieldInfo CustomAttributeField = typeof(ClientMethodInfo).GetField("customAttribute", BindingFlags.Instance | BindingFlags.NonPublic);
        public readonly static FieldInfo ReadMethodField = typeof(ClientMethodInfo).GetField("readMethod", BindingFlags.Instance | BindingFlags.NonPublic);
        public readonly static FieldInfo WriteMethodInfoField = typeof(ClientMethodInfo).GetField("writeMethodInfo", BindingFlags.Instance | BindingFlags.NonPublic);
        public readonly static FieldInfo MethodIndexField = typeof(ClientMethodInfo).GetField("methodIndex", BindingFlags.Instance | BindingFlags.NonPublic);
    }

    private static List<ClientMethodInfo> ClientMethods = 
        (List<ClientMethodInfo>)typeof(NetReflection).GetField("clientMethods", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);

    private static List<ServerMethodInfo> ServerMethods = 
        (List<ServerMethodInfo>)typeof(NetReflection).GetField("serverMethods", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);

    private static FieldInfo ClientMethodsLengthField = typeof(NetReflection).GetField("clientMethodsLength", BindingFlags.Static | BindingFlags.NonPublic);
    private static FieldInfo ClientMethodsBitCountField = typeof(NetReflection).GetField("clientMethodsBitCount", BindingFlags.Static | BindingFlags.NonPublic);

    private static FieldInfo ServerMethodsLengthField = typeof(NetReflection).GetField("serverMethodsLength", BindingFlags.Static | BindingFlags.NonPublic);
    private static FieldInfo ServerMethodsBitCountField = typeof(NetReflection).GetField("serverMethodsBitCount", BindingFlags.Static | BindingFlags.NonPublic);

    private readonly struct NetMethodData
    {
        public readonly Type DeclaringType;
        public readonly NetMethodAttribute MethodAttribute;

        public NetMethodData(Type declaringType, NetMethodAttribute methodAttribute)
        {
            DeclaringType = declaringType;
            MethodAttribute = methodAttribute;
        }
    }

    private static SteamCall GetSteamCall(NetMethodCaller caller) => caller switch
    {
        NetMethodCaller.ServerCaller => new(ESteamCallValidation.ONLY_FROM_SERVER),
        NetMethodCaller.ClientCaller => new(ESteamCallValidation.ONLY_FROM_OWNER),
        _ => new(ESteamCallValidation.NONE)
    };
    
    private const BindingFlags BindingFlagsMask = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
    private static void ConstructClientNetMethod(NetMethodData data)
    {
        MethodInfo writeInfo = data.DeclaringType.GetMethod(data.MethodAttribute.WriteMethodName, BindingFlagsMask);
        if (writeInfo == null)
        {
            return;
        }

        MethodInfo readInfo = data.DeclaringType.GetMethod(data.MethodAttribute.ReadMethodName, BindingFlagsMask);
        if (writeInfo == null)
        {
            return; }

        SteamCall call = GetSteamCall(data.MethodAttribute.AllowedCaller);
        
        ClientMethodReceive readDelegate = (in ClientInvocationContext context) => 
        {
            readInfo.Invoke(null, new object[] { context.reader });
        };

        ClientMethodInfo info = new();
        ClientMethodInfoFields.DeclaringTypeField.SetValue(info, data.DeclaringType);
        ClientMethodInfoFields.NameField.SetValue(info, data.MethodAttribute.Name);
        ClientMethodInfoFields.DebugNameField.SetValue(info, data.DeclaringType.FullName);
        ClientMethodInfoFields.CustomAttributeField.SetValue(info, call);
        ClientMethodInfoFields.WriteMethodInfoField.SetValue(info, writeInfo);
        ClientMethodInfoFields.ReadMethodField.SetValue(info, readDelegate);
        // Needs to be uint lest we explode and die
        ClientMethodInfoFields.MethodIndexField.SetValue(info, (uint)ClientMethods.Count);
        ClientMethods.Add(info);
    }

    // To do add rate limit support
    private static void ConstructServerNetMethod(NetMethodData data)
    {
        MethodInfo writeInfo = data.DeclaringType.GetMethod(data.MethodAttribute.WriteMethodName, BindingFlagsMask);
        if (writeInfo == null)
        {
            return;
        }

        MethodInfo readInfo = data.DeclaringType.GetMethod(data.MethodAttribute.ReadMethodName, BindingFlagsMask);
        if (writeInfo == null)
        {
            return; 
        }

        SteamCall call = GetSteamCall(data.MethodAttribute.AllowedCaller);
        
        ServerMethodReceive readDelegate = (in ServerInvocationContext context) => 
        {
            readInfo.Invoke(null, new object[] { context.reader });
        };

        ServerMethodInfo info = new();
        ServerMethodInfoFields.DeclaringTypeField.SetValue(info, data.DeclaringType);
        ServerMethodInfoFields.NameField.SetValue(info, data.MethodAttribute.Name);
        ServerMethodInfoFields.DebugNameField.SetValue(info, data.DeclaringType.FullName);
        ServerMethodInfoFields.CustomAttributeField.SetValue(info, call);
        ServerMethodInfoFields.WriteMethodInfoField.SetValue(info, writeInfo);
        ServerMethodInfoFields.ReadMethodField.SetValue(info, readDelegate);
        // Needs to be uint lest we explode and die
        ServerMethodInfoFields.MethodIndexField.SetValue(info, (uint)ServerMethods.Count);
        ServerMethods.Add(info);
    }

    public static void RegisterNetMethods(Assembly assembly)
    {
        HashSet<NetMethodData> methods = new();

        foreach (Type type in assembly.GetTypes())
        {
            IEnumerable<NetMethodAttribute> attributes = type.GetCustomAttributes<NetMethodAttribute>();
            if (attributes == null || attributes.Count() == 0)
            {
                continue;
            }

            foreach (NetMethodAttribute attribute in attributes)
            {
                NetMethodData data = new(type, attribute);
                methods.Add(data);
            }
        }

        foreach (NetMethodData data in methods)
        {
            if (data.MethodAttribute.AllowedCaller == NetMethodCaller.ServerCaller)
            {
                ConstructClientNetMethod(data);
                continue;
            }

            if (data.MethodAttribute.AllowedCaller == NetMethodCaller.ClientCaller)
            {
                ConstructServerNetMethod(data);
                continue;
            }
        }

        ClientMethodsLengthField.SetValue(null, (uint)ServerMethods.Count);
        ClientMethodsBitCountField.SetValue(null, NetPakConst.CountBits((uint)ServerMethods.Count));

        ServerMethodsLengthField.SetValue(null, (uint)ServerMethods.Count);
        ServerMethodsBitCountField.SetValue(null, NetPakConst.CountBits((uint)ServerMethods.Count));
    }
}
