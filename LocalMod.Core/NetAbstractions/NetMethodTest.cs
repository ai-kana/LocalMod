using LocalMod.Core.Logging;
using Microsoft.Extensions.Logging;
using SDG.NetPak;
using SDG.Unturned;

namespace LocalMod.NetAbstractions;

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
