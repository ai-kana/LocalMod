using System.Collections;
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

    private static MethodInfo FindClientReceiveMethod = typeof(NetReflection).GetMethod("FindClientReceiveMethod", BindingFlags.Static | BindingFlags.NonPublic);
    private static MethodInfo FindAndRemoveGeneratedMethod = typeof(NetReflection).GetMethod("FindAndRemoveGeneratedMethod", BindingFlags.Static | BindingFlags.NonPublic);

    private static MethodInfo FindServerReceiveMethod = typeof(NetReflection).GetMethod("FindServerReceiveMethod", BindingFlags.Static | BindingFlags.NonPublic);

    private static List<ClientMethodInfo> ClientMethods = 
        (List<ClientMethodInfo>)typeof(NetReflection).GetField("clientMethods", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);

    private static List<ServerMethodInfo> ServerMethods = 
        (List<ServerMethodInfo>)typeof(NetReflection).GetField("serverMethods", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);

    private static FieldInfo ClientMethodsLengthField = typeof(NetReflection).GetField("clientMethodsLength", BindingFlags.Static | BindingFlags.NonPublic);
    private static FieldInfo ClientMethodsBitCountField = typeof(NetReflection).GetField("clientMethodsBitCount", BindingFlags.Static | BindingFlags.NonPublic);

    private static FieldInfo ServerMethodsLengthField = typeof(NetReflection).GetField("serverMethodsLength", BindingFlags.Static | BindingFlags.NonPublic);
    private static FieldInfo ServerMethodsBitCountField = typeof(NetReflection).GetField("serverMethodsBitCount", BindingFlags.Static | BindingFlags.NonPublic);

    // Generated Method type and fields
    private readonly static Type GeneratedMethodType = typeof(NetReflection).GetNestedType("GeneratedMethod", BindingFlags.NonPublic);
    private readonly static FieldInfo GeneratedMethodInfoField = GeneratedMethodType.GetField("info", BindingFlags.Instance | BindingFlags.Public);
    private readonly static FieldInfo GeneratedMethodAttributeField = GeneratedMethodType.GetField("attribute", BindingFlags.Instance | BindingFlags.Public);

    // Tried to clean up the local names as much as I could :/
    // Later: Break this up into more methods
    private static void ParseMethods(MethodInfo methodInfo, Type type, IList readMethods, IList writeMethods, ILogger logger)
    {
        SteamCall steamCallerAttribute = methodInfo.GetCustomAttribute<SteamCall>();
        if (steamCallerAttribute == null)
        {
            return;
        }
        ParameterInfo[] parameters = methodInfo.GetParameters();
        if (steamCallerAttribute.validation == ESteamCallValidation.ONLY_FROM_SERVER)
        {
            ClientMethodInfo clientMethodInfo = new ClientMethodInfo();
            ClientMethodInfoFields.DeclaringTypeField.SetValue(clientMethodInfo, methodInfo.DeclaringType);
            ClientMethodInfoFields.DebugNameField.SetValue(clientMethodInfo, $"{methodInfo.DeclaringType}.{methodInfo.Name}");
            ClientMethodInfoFields.NameField.SetValue(clientMethodInfo, methodInfo.Name);
            ClientMethodInfoFields.CustomAttributeField.SetValue(clientMethodInfo, steamCallerAttribute);
            bool flag = parameters.Length == 1 && parameters[0].ParameterType.GetElementType() == typeof(ClientInvocationContext);
            if (methodInfo.IsStatic && flag)
            {
                Delegate? newDelegate = Delegate.CreateDelegate(typeof(ClientMethodReceive), methodInfo, throwOnBindFailure: false) as ClientMethodReceive;
                ClientMethodInfoFields.ReadMethodField.SetValue(clientMethodInfo, newDelegate);
            }
            else
            {
                object name = ClientMethodInfoFields.NameField.GetValue(clientMethodInfo);
                ClientMethodInfoFields.ReadMethodField.SetValue(clientMethodInfo, FindClientReceiveMethod.Invoke(null, new object[] {type, readMethods, name}));
                if (!flag)
                {
                    object foundMethod = Activator.CreateInstance(GeneratedMethodType);
                    if ((bool)FindAndRemoveGeneratedMethod.Invoke(null, new object[] {writeMethods, methodInfo.Name, foundMethod}))
                    {
                        ClientMethodInfoFields.WriteMethodInfoField.SetValue(clientMethodInfo, GeneratedMethodInfoField.GetValue(foundMethod));
                    }
                    else
                    {
                        logger.LogInformation($"Unable to find client {type.Name}.{methodInfo.Name} write implementation");
                    }
                }
            }

            ClientMethodInfoFields.MethodIndexField.SetValue(clientMethodInfo, (uint)ClientMethods.Count);
            ClientMethods.Add(clientMethodInfo);
        }
        else
        {
            if (steamCallerAttribute.validation != ESteamCallValidation.SERVERSIDE && steamCallerAttribute.validation != ESteamCallValidation.ONLY_FROM_OWNER)
            {
                return;
            }

            ServerMethodInfo serverMethodInfo = new ServerMethodInfo();
            ServerMethodInfoFields.DeclaringTypeField.SetValue(serverMethodInfo, methodInfo.DeclaringType);
            ServerMethodInfoFields.NameField.SetValue(serverMethodInfo, methodInfo.Name);
            ServerMethodInfoFields.DebugNameField.SetValue(serverMethodInfo, $"{methodInfo.DeclaringType}.{methodInfo.Name}");
            ServerMethodInfoFields.CustomAttributeField.SetValue(serverMethodInfo, steamCallerAttribute);
            bool flag2 = parameters.Length == 1 && parameters[0].ParameterType.GetElementType() == typeof(ServerInvocationContext);
            if (methodInfo.IsStatic && flag2)
            {
                Delegate? newDelegate = Delegate.CreateDelegate(typeof(ServerMethodReceive), methodInfo, throwOnBindFailure: false) as ServerMethodReceive;
                ServerMethodInfoFields.ReadMethodField.SetValue(serverMethodInfo, newDelegate);
            }
            else
            {
                ServerMethodInfoFields.ReadMethodField.SetValue(serverMethodInfo, FindServerReceiveMethod.Invoke(null, new object[] {type, readMethods, methodInfo.Name}));
                if (!flag2)
                {
                    object foundMethod2 = Activator.CreateInstance(GeneratedMethodType);
                    if ((bool)FindAndRemoveGeneratedMethod.Invoke(null, new object[] {writeMethods, methodInfo.Name, foundMethod2}))
                    {
                        //serverMethodInfo.writeMethodInfo = foundMethod2.info;
                        ServerMethodInfoFields.WriteMethodInfoField.SetValue(serverMethodInfo, GeneratedMethodInfoField.GetValue(foundMethod2));
                    }
                    else
                    {
                        logger.LogInformation($"Unable to find server {type.Name}.{methodInfo.Name} write implementation");
                    }
                }
            }
            if (steamCallerAttribute.ratelimitHz > 0)
            {
                int index = (int)ServerMethodInfoFields.RateLimitIndexField.GetValue(null);
                ServerMethodInfoFields.RateLimitIndexField.SetValue(serverMethodInfo, index);
                steamCallerAttribute.rateLimitIndex = index;
                steamCallerAttribute.ratelimitSeconds = 1f / (float)steamCallerAttribute.ratelimitHz;
                index++;
                ServerMethodInfoFields.RateLimitedMethodsCountField.SetValue(null, index);
            }
            else
            {
                ServerMethodInfoFields.RateLimitIndexField.SetValue(serverMethodInfo, -1);
            }

            ServerMethodInfoFields.MethodIndexField.SetValue(serverMethodInfo, (uint)ServerMethods.Count);
            ServerMethods.Add(serverMethodInfo);
        }
    }

    internal static void RegisterFromType(IEnumerable<Type> types)
    {
        ILogger logger = Logger.CreateLogger<NetMethodManager>();

        foreach (Type type in types)
        {
            if (!type.IsClass || !type.IsAbstract)
            {
                continue; }
            NetInvokableGeneratedClassAttribute generatedClassAttribute = type.GetCustomAttribute<NetInvokableGeneratedClassAttribute>();
            if (generatedClassAttribute == null)
            {
                continue;
            }
            IList readMethods = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(GeneratedMethodType));
            IList writeMethods = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(GeneratedMethodType));
            MethodInfo[] methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public);
            foreach (MethodInfo methodInfo in methods)
            {
                NetInvokableGeneratedMethodAttribute generateMethodAttribute = methodInfo.GetCustomAttribute<NetInvokableGeneratedMethodAttribute>();
                if (generateMethodAttribute == null)
                {
                    continue;
                }

                object generatedMethod = Activator.CreateInstance(GeneratedMethodType);
                GeneratedMethodInfoField.SetValue(generatedMethod, methodInfo);
                GeneratedMethodAttributeField.SetValue(generatedMethod, generateMethodAttribute);
                switch (generateMethodAttribute.purpose)
                {
                    case ENetInvokableGeneratedMethodPurpose.Read:
                        readMethods.Add(generatedMethod);
                        break;
                    case ENetInvokableGeneratedMethodPurpose.Write:
                        writeMethods.Add(generatedMethod);
                        break;
                    default:
                        logger.LogWarning($"Generated method {type.Name}.{methodInfo.Name} unknown purpose {generateMethodAttribute.purpose}");
                        break;
                }
            }

            methods = generatedClassAttribute.targetType.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
            foreach (MethodInfo methodInfo in methods)
            {
                ParseMethods(methodInfo, type, readMethods, writeMethods, logger);
            }
        }

        ClientMethodsLengthField.SetValue(null, (uint)ClientMethods.Count);
        ClientMethodsBitCountField.SetValue(null, NetPakConst.CountBits((uint)ClientMethods.Count));
        ServerMethodsLengthField.SetValue(null, (uint)ServerMethods.Count);
        ServerMethodsBitCountField.SetValue(null, NetPakConst.CountBits((uint)ServerMethods.Count));
    }





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

    public static void GetNetMethods(Assembly assembly)
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

        ClientMethodsLengthField.SetValue(null, (uint)ClientMethods.Count);
        ClientMethodsBitCountField.SetValue(null, NetPakConst.CountBits((uint)ClientMethods.Count));
    }
}
