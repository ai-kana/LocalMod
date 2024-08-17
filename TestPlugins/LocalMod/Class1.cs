using Cysharp.Threading.Tasks;
using LocalMod.Logging;
using LocalMod.Plugins;
using Microsoft.Extensions.Logging;

using SDG.NetPak;
using SDG.Unturned;

namespace TestPlugin;

public class TestPlugin : IPlugin
{
    public string Name => "TestPlugin";
    public string Author => "Me";
    public string Description => "Plugin for testing";

    private ILogger? _Logger;

    public UniTask LoadAsync()
    {
        _Logger = Logger.CreateLogger<TestPlugin>();
        return UniTask.CompletedTask;
    }

    public UniTask UnloadAsync()
    {
        return UniTask.CompletedTask;
    }
}

public class Test
{
    public static readonly ClientInstanceMethod<string> SendServerLog = ClientInstanceMethod<string>.Get(typeof(Test), "ReceiveServerLog");

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
    public static void ReceiveServerLog(string message)
    {
        UnturnedLog.info(message);
    }
}

[NetInvokableGeneratedClass(typeof(Test))]
public static class TestRPC
{
    [NetInvokableGeneratedMethod("ReceiveServerLog", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveServerLog_Read(in ClientInvocationContext context)
    {
        ILogger logger = Logger.CreateLogger("TestRPC");

        NetPakReader reader = context.reader;
        reader.ReadNetId(out _);

        reader.ReadString(out string message);
        logger.LogInformation(message);
    }

    [NetInvokableGeneratedMethod("ReceiveServerLog", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveServerLog_Write(NetPakWriter writer, string message)
    {
        writer.WriteString(message);
    }
}
