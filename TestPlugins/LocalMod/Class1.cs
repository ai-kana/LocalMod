using Cysharp.Threading.Tasks;
using LocalMod.Core.NetAbstractions;
using LocalMod.Core.Plugins;
using SDG.NetPak;
using SDG.Unturned;

namespace TestPlugin;

public class TestPlugin : IPlugin
{
    public string Name => "TestPlugin";
    public string Author => "Me";
    public string Description => "Plugin for testing";

    public UniTask LoadAsync()
    {
        return UniTask.CompletedTask;
    }

    public UniTask UnloadAsync()
    {
        return UniTask.CompletedTask;
    }
}

[NetMethod(NetMethodCaller.ServerCaller, Name, nameof(Read), nameof(Write))]
public class NetMethodTest
{
    public const string Name = "SendLog";

    public static void Read(NetPakReader reader)
    {
        reader.ReadString(out string message);
        UnturnedLog.info(message);
    }

    public static void Write(NetPakWriter writer, string message)
    {
        writer.WriteString(message);
    }
}
