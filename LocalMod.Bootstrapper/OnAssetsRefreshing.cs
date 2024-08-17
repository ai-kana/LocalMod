using System.Reflection;
using HarmonyLib;
using SDG.Unturned;
using Action = System.Action;

namespace LocalMod.Bootstrapper;

// Easy but non-optimal way to get a reload request
[HarmonyPatch(typeof(Assets), "RequestReloadAllAssets")]
internal static class OnAssetsRefreshingPatch
{
    private static readonly FieldInfo HasFinishedLoading = 
        typeof(Assets).GetField("hasFinishedInitialStartupLoading", BindingFlags.Static | BindingFlags.NonPublic);

    public static Action? OnAssetsRefreshing;

    [HarmonyPrefix]
    public static void RequestReload()
    {
        bool hasFinished = (bool)HasFinishedLoading.GetValue(null);
        if (hasFinished && !Assets.isLoading)
        {
            OnAssetsRefreshing?.Invoke();
        }
    }
}
