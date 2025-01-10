using SDG.NetPak;
using SDG.NetTransport;
using SDG.Unturned;

namespace LocalMod.API.NetAbstractions;

internal static class ServerNetWrite
{
    private static void SendWrite(NetPakWriter writer, ITransportConnection connection, ENetReliability reliability)
    {
        connection.Send(writer.buffer, writer.writeByteIndex, reliability);
    }

    public static void WriteToClient(INetMethod method, NetPakWriter writer, ITransportConnection connection, ENetReliability reliability)
    {
        writer.Flush();
        SendWrite(writer, connection, reliability);
        InvokeLoopback(writer, method);
    }

    public static void WriteToClients(INetMethod method, NetPakWriter writer, IEnumerable<ITransportConnection> connections, ENetReliability reliability)
    {
        writer.Flush();
        foreach (ITransportConnection connection in connections)
        {
            SendWrite(writer, connection, reliability);
        }
        InvokeLoopback(writer, method);
    }

    public static void WriteToAllClients(INetMethod method, NetPakWriter writer, ENetReliability reliability)
    {
        WriteToClients(method, writer, Provider.clients.Select(x => x.transportConnection), reliability);
    }

    public static void InvokeLoopback(NetPakWriter writer, INetMethod method)
    {
        NetPakReader reader = NetMessages.Reader;
        reader.SetBufferSegmentCopy(writer.buffer, NetMessages.ProviderBuffer, writer.writeByteIndex);
        reader.Reset();
        reader.ReadBits(5, out _);
        reader.ReadUInt32(out _);
        InvocationData data = new(reader);
        try
        {
            method.ReceiveInvoke(data);
        }
        catch (Exception e)
        {
            UnturnedLog.exception(e, $"Exception invoking {method.NetMethodId} by client loopback:");
        }
    }
}

public abstract class ServerNetMethod : INetMethod
{
    public NetMethodCaller AllowedCaller => NetMethodCaller.ServerCaller;

    public abstract uint NetMethodId {get;}
    public virtual float RateLimit => 0;

    public abstract void SendInvoke(NetPakWriter writer);
    public abstract void ReceiveInvoke(ClientInvocationData data);

    public void ReceiveInvoke(InvocationData data)
    {
        ReceiveInvoke(new ClientInvocationData(data));
    }

    public void Invoke(ENetReliability reliability = ENetReliability.Reliable)
    {
        NetPakWriter writer = NetMessages.GetClientWriter(this);
        SendInvoke(writer);
        ServerNetWrite.WriteToAllClients(this, writer, reliability);
    }

    public void Invoke(ITransportConnection connection, ENetReliability reliability = ENetReliability.Reliable)
    {
        NetPakWriter writer = NetMessages.GetClientWriter(this);
        SendInvoke(writer);
        ServerNetWrite.WriteToClient(this, writer, connection, reliability);
    }

    public void Invoke(IEnumerable<ITransportConnection> connections, ENetReliability reliability = ENetReliability.Reliable)
    {
        NetPakWriter writer = NetMessages.GetClientWriter(this);
        SendInvoke(writer);
        ServerNetWrite.WriteToClients(this, writer, connections, reliability);
    }
}

public abstract class ServerNetMethod<T1> : INetMethod
{
    public NetMethodCaller AllowedCaller => NetMethodCaller.ServerCaller;

    public abstract uint NetMethodId {get;}
    public float RateLimit => 0;

    public abstract void SendInvoke(NetPakWriter writer, T1 arg1);
    public abstract void ReceiveInvoke(ClientInvocationData data);

    public void ReceiveInvoke(InvocationData data)
    {
        ReceiveInvoke(new ClientInvocationData(data));
    }

    public void Invoke(T1 arg1, ENetReliability reliability = ENetReliability.Reliable)
    {
        NetPakWriter writer = NetMessages.GetClientWriter(this);
        SendInvoke(writer, arg1);
        ServerNetWrite.WriteToAllClients(this, writer, reliability);
    }

    public void Invoke(T1 arg1, ITransportConnection connection, ENetReliability reliability = ENetReliability.Reliable)
    {
        NetPakWriter writer = NetMessages.GetClientWriter(this);
        SendInvoke(writer, arg1);
        ServerNetWrite.WriteToClient(this, writer, connection, reliability);
    }

    public void Invoke(T1 arg1, IEnumerable<ITransportConnection> connections, ENetReliability reliability = ENetReliability.Reliable)
    {
        NetPakWriter writer = NetMessages.GetClientWriter(this);
        SendInvoke(writer, arg1);
        ServerNetWrite.WriteToClients(this, writer, connections, reliability);
    }
}

public abstract class ServerNetMethod<T1, T2> : INetMethod
{
    public NetMethodCaller AllowedCaller => NetMethodCaller.ServerCaller;

    public abstract uint NetMethodId {get;}
    public float RateLimit => 0;

    public abstract void SendInvoke(NetPakWriter writer, T1 arg1, T2 arg2);
    public abstract void ReceiveInvoke(ClientInvocationData data);

    public void ReceiveInvoke(InvocationData data)
    {
        ReceiveInvoke(new ClientInvocationData(data));
    }

    public void Invoke(T1 arg1, T2 arg2, ENetReliability reliability = ENetReliability.Reliable)
    {
        NetPakWriter writer = NetMessages.GetClientWriter(this);
        SendInvoke(writer, arg1, arg2);
        ServerNetWrite.WriteToAllClients(this, writer, reliability);
    }

    public void Invoke(T1 arg1, T2 arg2, ITransportConnection connection, ENetReliability reliability = ENetReliability.Reliable)
    {
        NetPakWriter writer = NetMessages.GetClientWriter(this);
        SendInvoke(writer, arg1, arg2);
        ServerNetWrite.WriteToClient(this, writer, connection, reliability);
    }

    public void Invoke(T1 arg1, T2 arg2, IEnumerable<ITransportConnection> connections, ENetReliability reliability = ENetReliability.Reliable)
    {
        NetPakWriter writer = NetMessages.GetClientWriter(this);
        SendInvoke(writer, arg1, arg2);
        ServerNetWrite.WriteToClients(this, writer, connections, reliability);
    }
}

public abstract class ServerNetMethod<T1, T2, T3> : INetMethod
{
    public NetMethodCaller AllowedCaller => NetMethodCaller.ServerCaller;

    public abstract uint NetMethodId {get;}
    public float RateLimit => 0;

    public abstract void SendInvoke(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3);
    public abstract void ReceiveInvoke(ClientInvocationData data);

    public void ReceiveInvoke(InvocationData data)
    {
        ReceiveInvoke(new ClientInvocationData(data));
    }

    public void Invoke(T1 arg1, T2 arg2, T3 arg3, ENetReliability reliability = ENetReliability.Reliable)
    {
        NetPakWriter writer = NetMessages.GetClientWriter(this);
        SendInvoke(writer, arg1, arg2, arg3);
        ServerNetWrite.WriteToAllClients(this, writer, reliability);
    }

    public void Invoke(T1 arg1, T2 arg2, T3 arg3, ITransportConnection connection, ENetReliability reliability = ENetReliability.Reliable)
    {
        NetPakWriter writer = NetMessages.GetClientWriter(this);
        SendInvoke(writer, arg1, arg2, arg3);
        ServerNetWrite.WriteToClient(this, writer, connection, reliability);
    }

    public void Invoke(T1 arg1, T2 arg2, T3 arg3, IEnumerable<ITransportConnection> connections, ENetReliability reliability = ENetReliability.Reliable)
    {
        NetPakWriter writer = NetMessages.GetClientWriter(this);
        SendInvoke(writer, arg1, arg2, arg3);
        ServerNetWrite.WriteToClients(this, writer, connections, reliability);
    }
}

public abstract class ServerNetMethod<T1, T2, T3, T4> : INetMethod
{
    public NetMethodCaller AllowedCaller => NetMethodCaller.ServerCaller;

    public abstract uint NetMethodId {get;}
    public float RateLimit => 0;

    public abstract void SendInvoke(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4);
    public abstract void ReceiveInvoke(ClientInvocationData data);

    public void ReceiveInvoke(InvocationData data)
    {
        ReceiveInvoke(new ClientInvocationData(data));
    }

    public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, ENetReliability reliability = ENetReliability.Reliable)
    {
        NetPakWriter writer = NetMessages.GetClientWriter(this);
        SendInvoke(writer, arg1, arg2, arg3, arg4);
        ServerNetWrite.WriteToAllClients(this, writer, reliability);
    }

    public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, ITransportConnection connection, ENetReliability reliability = ENetReliability.Reliable)
    {
        NetPakWriter writer = NetMessages.GetClientWriter(this);
        SendInvoke(writer, arg1, arg2, arg3, arg4);
        ServerNetWrite.WriteToClient(this, writer, connection, reliability);
    }

    public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, IEnumerable<ITransportConnection> connections, ENetReliability reliability = ENetReliability.Reliable)
    {
        NetPakWriter writer = NetMessages.GetClientWriter(this);
        SendInvoke(writer, arg1, arg2, arg3, arg4);
        ServerNetWrite.WriteToClients(this, writer, connections, reliability);
    }
}

public abstract class ServerNetMethod<T1, T2, T3, T4, T5> : INetMethod
{
    public NetMethodCaller AllowedCaller => NetMethodCaller.ServerCaller;

    public abstract uint NetMethodId {get;}
    public float RateLimit => 0;

    public abstract void SendInvoke(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
    public abstract void ReceiveInvoke(ClientInvocationData data);

    public void ReceiveInvoke(InvocationData data)
    {
        ReceiveInvoke(new ClientInvocationData(data));
    }

    public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, ENetReliability reliability = ENetReliability.Reliable)
    {
        NetPakWriter writer = NetMessages.GetClientWriter(this);
        SendInvoke(writer, arg1, arg2, arg3, arg4, arg5);
        ServerNetWrite.WriteToAllClients(this, writer, reliability);
    }

    public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, ITransportConnection connection, ENetReliability reliability = ENetReliability.Reliable)
    {
        NetPakWriter writer = NetMessages.GetClientWriter(this);
        SendInvoke(writer, arg1, arg2, arg3, arg4, arg5);
        ServerNetWrite.WriteToClient(this, writer, connection, reliability);
    }

    public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, IEnumerable<ITransportConnection> connections, ENetReliability reliability = ENetReliability.Reliable)
    {
        NetPakWriter writer = NetMessages.GetClientWriter(this);
        SendInvoke(writer, arg1, arg2, arg3, arg4, arg5);
        ServerNetWrite.WriteToClients(this, writer, connections, reliability);
    }
}

public abstract class ServerNetMethod<T1, T2, T3, T4, T5, T6> : INetMethod
{
    public NetMethodCaller AllowedCaller => NetMethodCaller.ServerCaller;

    public abstract uint NetMethodId {get;}
    public float RateLimit => 0;

    public abstract void SendInvoke(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);
    public abstract void ReceiveInvoke(ClientInvocationData data);

    public void ReceiveInvoke(InvocationData data)
    {
        ReceiveInvoke(new ClientInvocationData(data));
    }

    public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, ENetReliability reliability = ENetReliability.Reliable)
    {
        NetPakWriter writer = NetMessages.GetClientWriter(this);
        SendInvoke(writer, arg1, arg2, arg3, arg4, arg5, arg6);
        ServerNetWrite.WriteToAllClients(this, writer, reliability);
    }

    public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, ITransportConnection connection, ENetReliability reliability = ENetReliability.Reliable)
    {
        NetPakWriter writer = NetMessages.GetClientWriter(this);
        SendInvoke(writer, arg1, arg2, arg3, arg4, arg5, arg6);
        ServerNetWrite.WriteToClient(this, writer, connection, reliability);
    }

    public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, IEnumerable<ITransportConnection> connections, ENetReliability reliability = ENetReliability.Reliable)
    {
        NetPakWriter writer = NetMessages.GetClientWriter(this);
        SendInvoke(writer, arg1, arg2, arg3, arg4, arg5, arg6);
        ServerNetWrite.WriteToClients(this, writer, connections, reliability);
    }
}

public abstract class ServerNetMethod<T1, T2, T3, T4, T5, T6, T7> : INetMethod
{
    public NetMethodCaller AllowedCaller => NetMethodCaller.ServerCaller;

    public abstract uint NetMethodId {get;}
    public float RateLimit => 0;

    public abstract void SendInvoke(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);
    public abstract void ReceiveInvoke(ClientInvocationData data);

    public void ReceiveInvoke(InvocationData data)
    {
        ReceiveInvoke(new ClientInvocationData(data));
    }

    public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, ENetReliability reliability = ENetReliability.Reliable)
    {
        NetPakWriter writer = NetMessages.GetClientWriter(this);
        SendInvoke(writer, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        ServerNetWrite.WriteToAllClients(this, writer, reliability);
    }

    public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, ITransportConnection connection, ENetReliability reliability = ENetReliability.Reliable)
    {
        NetPakWriter writer = NetMessages.GetClientWriter(this);
        SendInvoke(writer, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        ServerNetWrite.WriteToClient(this, writer, connection, reliability);
    }

    public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, IEnumerable<ITransportConnection> connections, ENetReliability reliability = ENetReliability.Reliable)
    {
        NetPakWriter writer = NetMessages.GetClientWriter(this);
        SendInvoke(writer, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        ServerNetWrite.WriteToClients(this, writer, connections, reliability);
    }
}

public abstract class ServerNetMethod<T1, T2, T3, T4, T5, T6, T7, T8> : INetMethod
{
    public NetMethodCaller AllowedCaller => NetMethodCaller.ServerCaller;

    public abstract uint NetMethodId {get;}
    public float RateLimit => 0;

    public abstract void SendInvoke(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8);
    public abstract void ReceiveInvoke(ClientInvocationData data);

    public void ReceiveInvoke(InvocationData data)
    {
        ReceiveInvoke(new ClientInvocationData(data));
    }

    public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, ENetReliability reliability = ENetReliability.Reliable)
    {
        NetPakWriter writer = NetMessages.GetClientWriter(this);
        SendInvoke(writer, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        ServerNetWrite.WriteToAllClients(this, writer, reliability);
    }

    public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, ITransportConnection connection, ENetReliability reliability = ENetReliability.Reliable)
    {
        NetPakWriter writer = NetMessages.GetClientWriter(this);
        SendInvoke(writer, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        ServerNetWrite.WriteToClient(this, writer, connection, reliability);
    }

    public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, IEnumerable<ITransportConnection> connections, ENetReliability reliability = ENetReliability.Reliable)
    {
        NetPakWriter writer = NetMessages.GetClientWriter(this);
        SendInvoke(writer, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        ServerNetWrite.WriteToClients(this, writer, connections, reliability);
    }
}

public abstract class ServerNetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9> : INetMethod
{
    public NetMethodCaller AllowedCaller => NetMethodCaller.ServerCaller;

    public abstract uint NetMethodId {get;}
    public float RateLimit => 0;

    public abstract void SendInvoke(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9);
    public abstract void ReceiveInvoke(ClientInvocationData data);

    public void ReceiveInvoke(InvocationData data)
    {
        ReceiveInvoke(new ClientInvocationData(data));
    }

    public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, ENetReliability reliability = ENetReliability.Reliable)
    {
        NetPakWriter writer = NetMessages.GetClientWriter(this);
        SendInvoke(writer, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
        ServerNetWrite.WriteToAllClients(this, writer, reliability);
    }

    public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, ITransportConnection connection, ENetReliability reliability = ENetReliability.Reliable)
    {
        NetPakWriter writer = NetMessages.GetClientWriter(this);
        SendInvoke(writer, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
        ServerNetWrite.WriteToClient(this, writer, connection, reliability);
    }

    public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, IEnumerable<ITransportConnection> connections, ENetReliability reliability = ENetReliability.Reliable)
    {
        NetPakWriter writer = NetMessages.GetClientWriter(this);
        SendInvoke(writer, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
        ServerNetWrite.WriteToClients(this, writer, connections, reliability);
    }
}

public abstract class ServerNetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : INetMethod
{
    public NetMethodCaller AllowedCaller => NetMethodCaller.ServerCaller;

    public abstract uint NetMethodId {get;}
    public float RateLimit => 0;

    public abstract void SendInvoke(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10);
    public abstract void ReceiveInvoke(ClientInvocationData data);

    public void ReceiveInvoke(InvocationData data)
    {
        ReceiveInvoke(new ClientInvocationData(data));
    }

    public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, ENetReliability reliability = ENetReliability.Reliable)
    {
        NetPakWriter writer = NetMessages.GetClientWriter(this);
        SendInvoke(writer, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
        ServerNetWrite.WriteToAllClients(this, writer, reliability);
    }

    public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, ITransportConnection connection, ENetReliability reliability = ENetReliability.Reliable)
    {
        NetPakWriter writer = NetMessages.GetClientWriter(this);
        SendInvoke(writer, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
        ServerNetWrite.WriteToClient(this, writer, connection, reliability);
    }

    public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, IEnumerable<ITransportConnection> connections, ENetReliability reliability = ENetReliability.Reliable)
    {
        NetPakWriter writer = NetMessages.GetClientWriter(this);
        SendInvoke(writer, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
        ServerNetWrite.WriteToClients(this, writer, connections, reliability);
    }
}
