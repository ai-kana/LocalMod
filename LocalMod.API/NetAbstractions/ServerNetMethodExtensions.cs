using SDG.NetTransport;
using SDG.Unturned;

namespace LocalMod.API.NetAbstractions;

public static class ServerNetMethodExtensions
{
    private static IEnumerable<ITransportConnection> GetConnections(IEnumerable<SteamPlayer> players)
    {
        foreach (SteamPlayer player in players)
        {
            yield return player.transportConnection;
        }
    }

    public static void Invoke(
            this ServerNetMethod instance, 
            IEnumerable<SteamPlayer> players, 
            ENetReliability reliability = ENetReliability.Reliable)
    {
        instance.Invoke(GetConnections(players), reliability);
    }

    public static void Invoke(
            this ServerNetMethod instance, 
            SteamPlayer player, 
            ENetReliability reliability = ENetReliability.Reliable)
    {
        instance.Invoke(player.transportConnection, reliability);
    }

    public static void Invoke<T1>(
            this ServerNetMethod<T1> instance, 
            T1 arg1,
            IEnumerable<SteamPlayer> players, 
            ENetReliability reliability = ENetReliability.Reliable)
    {
        instance.Invoke(arg1, GetConnections(players), reliability);
    }

    public static void Invoke<T1>(
            this ServerNetMethod<T1> instance, 
            T1 arg1,
            SteamPlayer player, 
            ENetReliability reliability = ENetReliability.Reliable)
    {
        instance.Invoke(arg1, player.transportConnection, reliability);
    }

    public static void Invoke<T1, T2>(
            this ServerNetMethod<T1, T2> instance, 
            T1 arg1,
            T2 arg2,
            IEnumerable<SteamPlayer> players, 
            ENetReliability reliability = ENetReliability.Reliable)
    {
        instance.Invoke(arg1, arg2, GetConnections(players), reliability);
    }

    public static void Invoke<T1, T2>(
            this ServerNetMethod<T1, T2> instance, 
            T1 arg1,
            T2 arg2,
            SteamPlayer player, 
            ENetReliability reliability = ENetReliability.Reliable)
    {
        instance.Invoke(arg1, arg2, player.transportConnection, reliability);
    }

    public static void Invoke<T1, T2, T3>(
            this ServerNetMethod<T1, T2, T3> instance, 
            T1 arg1,
            T2 arg2,
            T3 arg3,
            IEnumerable<SteamPlayer> players, 
            ENetReliability reliability = ENetReliability.Reliable)
    {
        instance.Invoke(arg1, arg2, arg3, GetConnections(players), reliability);
    }

    public static void Invoke<T1, T2, T3>(
            this ServerNetMethod<T1, T2, T3> instance, 
            T1 arg1,
            T2 arg2,
            T3 arg3,
            SteamPlayer player, 
            ENetReliability reliability = ENetReliability.Reliable)
    {
        instance.Invoke(arg1, arg2, arg3, player.transportConnection, reliability);
    }

    public static void Invoke<T1, T2, T3, T4>(
            this ServerNetMethod<T1, T2, T3, T4> instance, 
            T1 arg1,
            T2 arg2,
            T3 arg3,
            T4 arg4,
            IEnumerable<SteamPlayer> players, 
            ENetReliability reliability = ENetReliability.Reliable)
    {
        instance.Invoke(arg1, arg2, arg3, arg4, GetConnections(players), reliability);
    }

    public static void Invoke<T1, T2, T3, T4>(
            this ServerNetMethod<T1, T2, T3, T4> instance, 
            T1 arg1,
            T2 arg2,
            T3 arg3,
            T4 arg4,
            SteamPlayer player, 
            ENetReliability reliability = ENetReliability.Reliable)
    {
        instance.Invoke(arg1, arg2, arg3, arg4, player.transportConnection, reliability);
    }

    public static void Invoke<T1, T2, T3, T4, T5, T6>(
            this ServerNetMethod<T1, T2, T3, T4, T5, T6> instance, 
            T1 arg1,
            T2 arg2,
            T3 arg3,
            T4 arg4,
            T5 arg5,
            T6 arg6,
            IEnumerable<SteamPlayer> players, 
            ENetReliability reliability = ENetReliability.Reliable)
    {
        instance.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, GetConnections(players), reliability);
    }

    public static void Invoke<T1, T2, T3, T4, T5, T6>(
            this ServerNetMethod<T1, T2, T3, T4, T5, T6> instance, 
            T1 arg1,
            T2 arg2,
            T3 arg3,
            T4 arg4,
            T5 arg5,
            T6 arg6,
            SteamPlayer player, 
            ENetReliability reliability = ENetReliability.Reliable)
    {
        instance.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, player.transportConnection, reliability);
    }

    public static void Invoke<T1, T2, T3, T4, T5, T6, T7>(
            this ServerNetMethod<T1, T2, T3, T4, T5, T6, T7> instance, 
            T1 arg1,
            T2 arg2,
            T3 arg3,
            T4 arg4,
            T5 arg5,
            T6 arg6,
            T7 arg7,
            IEnumerable<SteamPlayer> players, 
            ENetReliability reliability = ENetReliability.Reliable)
    {
        instance.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, GetConnections(players), reliability);
    }

    public static void Invoke<T1, T2, T3, T4, T5, T6, T7>(
            this ServerNetMethod<T1, T2, T3, T4, T5, T6, T7> instance, 
            T1 arg1,
            T2 arg2,
            T3 arg3,
            T4 arg4,
            T5 arg5,
            T6 arg6,
            T7 arg7,
            SteamPlayer player, 
            ENetReliability reliability = ENetReliability.Reliable)
    {
        instance.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, player.transportConnection, reliability);
    }

    public static void Invoke<T1, T2, T3, T4, T5, T6, T7, T8>(
            this ServerNetMethod<T1, T2, T3, T4, T5, T6, T7, T8> instance, 
            T1 arg1,
            T2 arg2,
            T3 arg3,
            T4 arg4,
            T5 arg5,
            T6 arg6,
            T7 arg7,
            T8 arg8,
            IEnumerable<SteamPlayer> players, 
            ENetReliability reliability = ENetReliability.Reliable)
    {
        instance.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, GetConnections(players), reliability);
    }

    public static void Invoke<T1, T2, T3, T4, T5, T6, T7, T8>(
            this ServerNetMethod<T1, T2, T3, T4, T5, T6, T7, T8> instance, 
            T1 arg1,
            T2 arg2,
            T3 arg3,
            T4 arg4,
            T5 arg5,
            T6 arg6,
            T7 arg7,
            T8 arg8,
            SteamPlayer player, 
            ENetReliability reliability = ENetReliability.Reliable)
    {
        instance.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, player.transportConnection, reliability);
    }

    public static void Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            this ServerNetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9> instance, 
            T1 arg1,
            T2 arg2,
            T3 arg3,
            T4 arg4,
            T5 arg5,
            T6 arg6,
            T7 arg7,
            T8 arg8,
            T9 arg9,
            IEnumerable<SteamPlayer> players, 
            ENetReliability reliability = ENetReliability.Reliable)
    {
        instance.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, GetConnections(players), reliability);
    }

    public static void Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            this ServerNetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9> instance, 
            T1 arg1,
            T2 arg2,
            T3 arg3,
            T4 arg4,
            T5 arg5,
            T6 arg6,
            T7 arg7,
            T8 arg8,
            T9 arg9,
            SteamPlayer player, 
            ENetReliability reliability = ENetReliability.Reliable)
    {
        instance.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, player.transportConnection, reliability);
    }

    public static void Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            this ServerNetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> instance, 
            T1 arg1,
            T2 arg2,
            T3 arg3,
            T4 arg4,
            T5 arg5,
            T6 arg6,
            T7 arg7,
            T8 arg8,
            T9 arg9,
            T10 arg10,
            IEnumerable<SteamPlayer> players, 
            ENetReliability reliability = ENetReliability.Reliable)
    {
        instance.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, GetConnections(players), reliability);
    }

    public static void Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            this ServerNetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> instance, 
            T1 arg1,
            T2 arg2,
            T3 arg3,
            T4 arg4,
            T5 arg5,
            T6 arg6,
            T7 arg7,
            T8 arg8,
            T9 arg9,
            T10 arg10,
            SteamPlayer player, 
            ENetReliability reliability = ENetReliability.Reliable)
    {
        instance.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, player.transportConnection, reliability);
    }
}
