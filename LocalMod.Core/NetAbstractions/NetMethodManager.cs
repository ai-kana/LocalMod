using System.Collections;
using System.Reflection;
using LocalMod.Core.Logging;
using Microsoft.Extensions.Logging;
using SDG.NetPak;
using SDG.Unturned;

namespace LocalMod.NetAbstractions;

public class NetMethodManager
{
    public static void LogAllRPCs()
    {
        ILogger logger = Logger.CreateLogger<NetMethodManager>();
        using (logger.BeginScope("RPCs"))
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
        public static FieldInfo DeclaringTypeField = typeof(ServerMethodInfo).GetField("declaringType", BindingFlags.Instance | BindingFlags.NonPublic);
        public static FieldInfo NameField = typeof(ServerMethodInfo).GetField("name", BindingFlags.Instance | BindingFlags.NonPublic);
        public static FieldInfo DebugNameField = typeof(ServerMethodInfo).GetField("debugName", BindingFlags.Instance | BindingFlags.NonPublic);
        public static FieldInfo CustomAttributeField = typeof(ServerMethodInfo).GetField("customAttribute", BindingFlags.Instance | BindingFlags.NonPublic);
        public static FieldInfo ReadMethodField = typeof(ServerMethodInfo).GetField("readMethod", BindingFlags.Instance | BindingFlags.NonPublic);
        public static FieldInfo WriteMethodInfoField = typeof(ServerMethodInfo).GetField("writeMethodInfo", BindingFlags.Instance | BindingFlags.NonPublic);
        public static FieldInfo MethodIndexField = typeof(ServerMethodInfo).GetField("methodIndex", BindingFlags.Instance | BindingFlags.NonPublic);
        public static FieldInfo RateLimitIndexField = typeof(ServerMethodInfo).GetField("rateLimitIndex", BindingFlags.Instance | BindingFlags.NonPublic);
        public static FieldInfo RateLimitedMethodsCountField = typeof(ServerMethodInfo).GetField("rateLimitedMethodsCount", BindingFlags.Instance | BindingFlags.NonPublic);
    }

    private static class ClientMethodInfoFields
    {
        public static FieldInfo DeclaringTypeField = typeof(ClientMethodInfo).GetField("declaringType", BindingFlags.Instance | BindingFlags.NonPublic);
        public static FieldInfo NameField = typeof(ClientMethodInfo).GetField("name", BindingFlags.Instance | BindingFlags.NonPublic);
        public static FieldInfo DebugNameField = typeof(ClientMethodInfo).GetField("debugName", BindingFlags.Instance | BindingFlags.NonPublic);
        public static FieldInfo CustomAttributeField = typeof(ClientMethodInfo).GetField("customAttribute", BindingFlags.Instance | BindingFlags.NonPublic);
        public static FieldInfo ReadMethodField = typeof(ClientMethodInfo).GetField("readMethod", BindingFlags.Instance | BindingFlags.NonPublic);
        public static FieldInfo WriteMethodInfoField = typeof(ClientMethodInfo).GetField("writeMethodInfo", BindingFlags.Instance | BindingFlags.NonPublic);
        public static FieldInfo MethodIndexField = typeof(ClientMethodInfo).GetField("methodIndex", BindingFlags.Instance | BindingFlags.NonPublic);
    }

    // misc method
    private static MethodInfo FindClientReceiveMethod = typeof(NetReflection).GetMethod("FindClientReceiveMethod", BindingFlags.Static | BindingFlags.NonPublic);
    private static MethodInfo FindAndRemoveGeneratedMethod = typeof(NetReflection).GetMethod("FindAndRemoveGeneratedMethod", BindingFlags.Static | BindingFlags.NonPublic);

    private static MethodInfo FindServerReceiveMethod = typeof(NetReflection).GetMethod("FindServerReceiveMethod", BindingFlags.Static | BindingFlags.NonPublic);

    // misc fields
    private static List<ClientMethodInfo> ClientMethods = 
        (List<ClientMethodInfo>)typeof(NetReflection).GetField("clientMethods", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);

    private static List<ServerMethodInfo> ServerMethods = 
        (List<ServerMethodInfo>)typeof(NetReflection).GetField("serverMethods", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);

    private static FieldInfo ClientMethodsLengthField = typeof(NetReflection).GetField("clientMethodsLength", BindingFlags.Static | BindingFlags.NonPublic);
    private static FieldInfo ClientMethodsBitCountField = typeof(NetReflection).GetField("clientMethodsBitCount", BindingFlags.Static | BindingFlags.NonPublic);

    private static FieldInfo ServerMethodsLengthField = typeof(NetReflection).GetField("serverMethodsLength", BindingFlags.Static | BindingFlags.NonPublic);
    private static FieldInfo ServerMethodsBitCountField = typeof(NetReflection).GetField("serverMethodsBitCount", BindingFlags.Static | BindingFlags.NonPublic);

    private static Type GeneratedMethodType = typeof(NetReflection).GetNestedType("GeneratedMethod", BindingFlags.NonPublic);
    private static FieldInfo GeneratedMethodInfoField = GeneratedMethodType.GetField("info", BindingFlags.Instance | BindingFlags.Public);
    private static FieldInfo GeneratedMethodAttributeField = GeneratedMethodType.GetField("attribute", BindingFlags.Instance | BindingFlags.Public);

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
                continue;
            }
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
}
