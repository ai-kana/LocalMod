using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Cysharp.Threading.Tasks;
using OpenMod.Unturned.Plugins;
using OpenMod.API.Plugins;
using System.IO;
using System.Reflection;
using LocalMod.Core.NetAbstractions;

[assembly: PluginMetadata("LocalMod.Loader", DisplayName = "LocalModLoader")]
namespace LocalModLoader;

public class LocalModLoader : OpenModUnturnedPlugin
{
    private readonly IConfiguration _Configuration;
    private readonly IStringLocalizer _StringLocalizer;
    private readonly ILogger<LocalModLoader> _Logger;

    public LocalModLoader(
            IConfiguration configuration,
            IStringLocalizer stringLocalizer,
            ILogger<LocalModLoader> logger,
            IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _Configuration = configuration;
        _StringLocalizer = stringLocalizer;
        _Logger = logger;
    }

    protected override UniTask OnLoadAsync()
    {
        _Logger.LogInformation($"Registering NetRPCs...");

        string[] files = Directory.GetFiles(WorkingDirectory, "*.dll");
        foreach (string file in files)
        {
            Assembly assembly = Assembly.LoadFile(file);
            NetMethodManager.GetNetMethods(assembly);
            _Logger.LogInformation($"Loaded net invokables from: {assembly.FullName}");
        }

        _Logger.LogInformation($"Registered NetRPCs!");

        return UniTask.CompletedTask;
    }

    // Need to add RPC unloading here as well...
    protected override UniTask OnUnloadAsync()
    {
        return UniTask.CompletedTask;
    }
}
