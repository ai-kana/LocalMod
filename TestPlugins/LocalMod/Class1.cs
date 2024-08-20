using System.Reflection;
using Cysharp.Threading.Tasks;
using LocalMod.Core.NetAbstractions;
using LocalMod.Core.Plugins;
using SDG.NetPak;
using SDG.Unturned;
using Steamworks;
using UnityEngine;

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

[NetMethod(NetMethodCaller.ServerCaller, nameof(SendChatBulk), nameof(Read), nameof(Write))]
public class SendChatBulk
{
    private static void Read(NetPakReader reader)
    {
        reader.ReadInt32(out int count);
        for (int i = 0; i < count; i++)
        {
            reader.ReadString(out string message);
            ChatManager.receiveChatMessage(CSteamID.Nil, null, EChatMode.GLOBAL, Color.cyan, true, message);
        }
    }

    private static void Write(NetPakWriter writer, IEnumerable<string> messages)
    {
        writer.WriteInt32(messages.Count());
        foreach (string message in messages)
        {
            writer.WriteString(message);
        }
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
