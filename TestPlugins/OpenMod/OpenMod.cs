using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Cysharp.Threading.Tasks;
using OpenMod.Unturned.Plugins;
using OpenMod.API.Plugins;
using SDG.NetTransport;
using OpenMod.API.Eventing; 
using OpenMod.Unturned.Players.Connections.Events;
using System.Threading.Tasks;
using SDG.Unturned;
using System.Reflection;
using System.Collections.Generic;
using TestPlugin;
using UnityEngine;

// For more, visit https://openmod.github.io/openmod-docs/devdoc/guides/getting-started.html

[assembly: PluginMetadata("Kana.LocalModTest", DisplayName = "LocalModTest")]
namespace MyOpenModPlugin;

public class MyOpenModPlugin : OpenModUnturnedPlugin
{
    private readonly IConfiguration m_Configuration;
    private readonly IStringLocalizer m_StringLocalizer;
    private readonly ILogger<MyOpenModPlugin> m_Logger;

    public MyOpenModPlugin(
            IConfiguration configuration,
            IStringLocalizer stringLocalizer,
            ILogger<MyOpenModPlugin> logger,
            IServiceProvider serviceProvider) : base(serviceProvider)
    {
        m_Configuration = configuration;
        m_StringLocalizer = stringLocalizer;
        m_Logger = logger;
    }

    protected override UniTask OnLoadAsync()
    {
        FieldInfo info = typeof(NetReflection).GetField("clientMethods", BindingFlags.NonPublic | BindingFlags.Static);
        List<ClientMethodInfo> methods = (List<ClientMethodInfo>)info.GetValue(null);
        foreach (ClientMethodInfo method in methods)
        {
            m_Logger.LogInformation($"RPC: {method.ToString()}");
        }

        return UniTask.CompletedTask;
    }

    protected override UniTask OnUnloadAsync()
    {
        return UniTask.CompletedTask;
    }
}

public class PlayerConnected : IEventListener<UnturnedPlayerConnectedEvent>
{
    //private readonly static ClientInstanceMethod<string> SendLog = ClientInstanceMethod<string>.Get(typeof(NetMethodTest), NetMethodTest.Name);
    private readonly static ClientStaticMethod<string> SendLog = ClientStaticMethod<string>.Get(typeof(NetMethodTest), NetMethodTest.Name);

    private readonly static ClientStaticMethod<IEnumerable<string>> SendObject = 
        ClientStaticMethod<IEnumerable<string>>.Get(typeof(SendChatBulk), nameof(SendChatBulk));

    public Task HandleEventAsync(object? sender, UnturnedPlayerConnectedEvent @event)
    {
        Console.WriteLine("Starting event");
        ITransportConnection connection = @event.Player.SteamPlayer.transportConnection;
        NetId id = @event.Player.SteamPlayer.GetNetId();
        SendLog.Invoke(ENetReliability.Reliable, connection, "Hallo from server");

        List<string> messages = new();
        for (int i = 0; i < 10; i++)
        {
            messages.Add("Test:" + i.ToString());
        }
        SendObject.Invoke(ENetReliability.Reliable, connection, messages);

        return Task.CompletedTask;
    }
}
