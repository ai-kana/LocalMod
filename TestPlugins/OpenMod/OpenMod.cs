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

    protected override async UniTask OnLoadAsync()
    {
        FieldInfo info = typeof(NetReflection).GetField("clientMethods", BindingFlags.NonPublic | BindingFlags.Static);
        List<ClientMethodInfo> methods = (List<ClientMethodInfo>)info.GetValue(null);
        foreach (ClientMethodInfo method in methods)
        {
            m_Logger.LogInformation($"RPC: {method.ToString()}");
        }
        m_Logger.LogInformation("Wrote RPCs");
    }

    protected override async UniTask OnUnloadAsync()
    {
    }
}

public class PlayerConnected : IEventListener<UnturnedPlayerConnectedEvent>
{
    public async Task HandleEventAsync(object? sender, UnturnedPlayerConnectedEvent @event)
    {
        Console.WriteLine("Starting event");
        ITransportConnection connection = @event.Player.SteamPlayer.transportConnection;
        NetId id = @event.Player.SteamPlayer.GetNetId();

        if (TestPlugin.Test.SendServerLog == null)
        {
            Console.WriteLine("RPC null");
            return;
        }

        TestPlugin.Test.SendServerLog.Invoke(id, ENetReliability.Reliable, connection, "Hey faggot");
    }
}
