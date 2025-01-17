using SDG.NetTransport;
using SDG.Unturned;

namespace LocalMod.API.NetAbstractions;

public static class ClientNetMethodExtensions
{
    private static IEnumerable<ITransportConnection> GetConnections(IEnumerable<SteamPlayer> players)
    {
        foreach (SteamPlayer player in players)
        {
            yield return player.transportConnection;
        }
    }

    public static void Invoke(
            this ClientNetMethod instance, 
            ENetReliability reliability = ENetReliability.Reliable)
    {
        instance.Invoke(reliability);
    }

    public static void Invoke<T1>(
            this ClientNetMethod<T1> instance, 
            T1 arg1,
            ENetReliability reliability = ENetReliability.Reliable)
    {
        instance.Invoke(arg1, reliability);
    }

    public static void Invoke<T1, T2>(
            this ClientNetMethod<T1, T2> instance, 
            T1 arg1,
            T2 arg2,
            ENetReliability reliability = ENetReliability.Reliable)
    {
        instance.Invoke(arg1, arg2, reliability);
    }

    public static void Invoke<T1, T2, T3>(
            this ClientNetMethod<T1, T2, T3> instance, 
            T1 arg1,
            T2 arg2,
            T3 arg3,
            ENetReliability reliability = ENetReliability.Reliable)
    {
        instance.Invoke(arg1, arg2, arg3, reliability);
    }

    public static void Invoke<T1, T2, T3, T4>(
            this ClientNetMethod<T1, T2, T3, T4> instance, 
            T1 arg1,
            T2 arg2,
            T3 arg3,
            T4 arg4,
            ENetReliability reliability = ENetReliability.Reliable)
    {
        instance.Invoke(arg1, arg2, arg3, arg4, reliability);
    }

    public static void Invoke<T1, T2, T3, T4, T5, T6>(
            this ClientNetMethod<T1, T2, T3, T4, T5, T6> instance, 
            T1 arg1,
            T2 arg2,
            T3 arg3,
            T4 arg4,
            T5 arg5,
            T6 arg6,
            ENetReliability reliability = ENetReliability.Reliable)
    {
        instance.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, reliability);
    }

    public static void Invoke<T1, T2, T3, T4, T5, T6, T7>(
            this ClientNetMethod<T1, T2, T3, T4, T5, T6, T7> instance, 
            T1 arg1,
            T2 arg2,
            T3 arg3,
            T4 arg4,
            T5 arg5,
            T6 arg6,
            T7 arg7,
            ENetReliability reliability = ENetReliability.Reliable)
    {
        instance.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, reliability);
    }

    public static void Invoke<T1, T2, T3, T4, T5, T6, T7, T8>(
            this ClientNetMethod<T1, T2, T3, T4, T5, T6, T7, T8> instance, 
            T1 arg1,
            T2 arg2,
            T3 arg3,
            T4 arg4,
            T5 arg5,
            T6 arg6,
            T7 arg7,
            T8 arg8,
            ENetReliability reliability = ENetReliability.Reliable)
    {
        instance.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, reliability);
    }

    public static void Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            this ClientNetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9> instance, 
            T1 arg1,
            T2 arg2,
            T3 arg3,
            T4 arg4,
            T5 arg5,
            T6 arg6,
            T7 arg7,
            T8 arg8,
            T9 arg9,
            ENetReliability reliability = ENetReliability.Reliable)
    {
        instance.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, reliability);
    }

    public static void Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            this ClientNetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> instance, 
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
            ENetReliability reliability = ENetReliability.Reliable)
    {
        instance.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, reliability);
    }
}
