using SDG.NetPak;
using SDG.NetTransport;
using SDG.Unturned;
using LocalMod.API.Reflection;

namespace LocalMod.API.NetAbstractions;

internal static class NetMessages
{
    private readonly static NetPakWriter Writer = 
        typeof(Provider).Assembly
        .GetNonPublicType("NetMessages")
        .GetStaticField("writer")
        .GetValue<NetPakWriter>()
        ?? throw new("Failed to get netpak writer from net messages");

    public readonly static NetPakReader Reader = 
        typeof(Provider).Assembly
        .GetNonPublicType("NetMessages")
        .GetStaticField("reader")
        .GetValue<NetPakReader>()
        ?? throw new("Failed to get netpak reader from net messages");

    public readonly static byte[] ProviderBuffer =
        typeof(Provider)
        .GetStaticField("buffer")
        .GetValue<byte[]>()
        ?? throw new("Failed to get provider buffer");

    public static NetPakWriter GetServerWriter(INetMethod method)
    {
        NetPakWriter writer = NetMessages.Writer;
        writer.Reset();
        writer.WriteBits(6u, 4);
        // The below doesnt work for whatever reason
        // My best guess is something to do with the order of reading bits with little endian
        // The server only reads 3 bit but this writes 4
        // But so does above so what do I know :D
        // writer.WriteEnum(EServerMessage.InvokeMethod);
        writer.WriteUInt32(method.NetMethodId);
        return writer;
    }

    public static NetPakWriter GetClientWriter(INetMethod method)
    {
        NetPakWriter writer = NetMessages.Writer;
        writer.Reset();
        writer.WriteBits(17u, 5);
        writer.WriteUInt32(method.NetMethodId);
        return writer;
    }
}

internal static class ClientNetWrite
{    
    private readonly static IClientTransport ClientTransport =
        typeof(Provider)
        .GetStaticField("clientTransport")
        .GetValue<IClientTransport>();

    public static void Send(INetMethod method, NetPakWriter writer)
    {
        writer.Flush();
        if (!Provider.isConnected)
        {
            return;
        }

        if (!Provider.isServer)
        {
            ClientTransport.Send(writer.buffer, writer.writeByteIndex, ENetReliability.Reliable);
            return;
        }

        Loopback(writer, method);
    }

    private static void Loopback(NetPakWriter writer, INetMethod method)
    {
        NetPakReader reader = NetMessages.Reader;
        reader.SetBufferSegmentCopy(writer.buffer, NetMessages.ProviderBuffer, writer.writeByteIndex);
        reader.Reset();
        reader.ReadBits(4, out _);
        reader.ReadUInt32(out _);
        SteamPlayer caller = Provider.clients[0];
        InvocationData data = new(caller, reader);
        try
        {
            method.ReceiveInvoke(data);
        }
        catch (Exception e)
        {
            UnturnedLog.exception(e, $"Exception invoking {method.NetMethodId} by server loopback:");
        }
    }
}

public abstract class ClientNetMethod : INetMethod
{
    public NetMethodCaller AllowedCaller => NetMethodCaller.ClientCaller;

    public abstract uint NetMethodId {get;}

    public virtual float RateLimit => 0;

    public abstract void SendInvoke(NetPakWriter writer);
    public abstract void ReceiveInvoke(ServerInvocationData data);

    public void ReceiveInvoke(InvocationData data)
    {
        ReceiveInvoke(new ServerInvocationData(data));
    }

    public void Invoke()
    {
        NetPakWriter writer = NetMessages.GetServerWriter(this);
        SendInvoke(writer);
        ClientNetWrite.Send(this, writer);
    }
}

public abstract class ClientNetMethod<T1> : INetMethod
{
    public NetMethodCaller AllowedCaller => NetMethodCaller.ClientCaller;

    public abstract uint NetMethodId {get;}

    public virtual float RateLimit => 0;

    public abstract void SendInvoke(NetPakWriter writer, T1 arg1);
    public abstract void ReceiveInvoke(InvocationData reader);

    public void Invoke(T1 arg1)
    {
        NetPakWriter writer = NetMessages.GetServerWriter(this);
        SendInvoke(writer, arg1);
        ClientNetWrite.Send(this, writer);
    }
}

public abstract class ClientNetMethod<T1, T2> : INetMethod
{
    public NetMethodCaller AllowedCaller => NetMethodCaller.ClientCaller;

    public abstract uint NetMethodId {get;}

    public virtual float RateLimit => 0;

    public abstract void SendInvoke(NetPakWriter writer, T1 arg1, T2 arg2);
    public abstract void ReceiveInvoke(InvocationData reader);

    public void Invoke(T1 arg1, T2 arg2)
    {
        NetPakWriter writer = NetMessages.GetServerWriter(this);
        SendInvoke(writer, arg1, arg2);
        ClientNetWrite.Send(this, writer);
    }
}

public abstract class ClientNetMethod<T1, T2, T3> : INetMethod
{
    public NetMethodCaller AllowedCaller => NetMethodCaller.ClientCaller;

    public abstract uint NetMethodId {get;}

    public virtual float RateLimit => 0;

    public abstract void SendInvoke(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3);
    public abstract void ReceiveInvoke(InvocationData reader);

    public void Invoke(T1 arg1, T2 arg2, T3 arg3)
    {
        NetPakWriter writer = NetMessages.GetServerWriter(this);
        SendInvoke(writer, arg1, arg2, arg3);
        ClientNetWrite.Send(this, writer);
    }
}

public abstract class ClientNetMethod<T1, T2, T3, T4> : INetMethod
{
    public NetMethodCaller AllowedCaller => NetMethodCaller.ClientCaller;

    public abstract uint NetMethodId {get;}

    public virtual float RateLimit => 0;

    public abstract void SendInvoke(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4);
    public abstract void ReceiveInvoke(InvocationData reader);

    public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        NetPakWriter writer = NetMessages.GetServerWriter(this);
        SendInvoke(writer, arg1, arg2, arg3, arg4);
        ClientNetWrite.Send(this, writer);
    }
}

public abstract class ClientNetMethod<T1, T2, T3, T4, T5> : INetMethod
{
    public NetMethodCaller AllowedCaller => NetMethodCaller.ClientCaller;

    public abstract uint NetMethodId {get;}

    public virtual float RateLimit => 0;

    public abstract void SendInvoke(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
    public abstract void ReceiveInvoke(InvocationData reader);

    public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
    {
        NetPakWriter writer = NetMessages.GetServerWriter(this);
        SendInvoke(writer, arg1, arg2, arg3, arg4, arg5);
        ClientNetWrite.Send(this, writer);
    }
}

public abstract class ClientNetMethod<T1, T2, T3, T4, T5, T6> : INetMethod
{
    public NetMethodCaller AllowedCaller => NetMethodCaller.ClientCaller;

    public abstract uint NetMethodId {get;}

    public virtual float RateLimit => 0;

    public abstract void SendInvoke(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);
    public abstract void ReceiveInvoke(InvocationData reader);

    public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
    {
        NetPakWriter writer = NetMessages.GetServerWriter(this);
        SendInvoke(writer, arg1, arg2, arg3, arg4, arg5, arg6);
        ClientNetWrite.Send(this, writer);
    }
}

public abstract class ClientNetMethod<T1, T2, T3, T4, T5, T6, T7> : INetMethod
{
    public NetMethodCaller AllowedCaller => NetMethodCaller.ClientCaller;

    public abstract uint NetMethodId {get;}

    public virtual float RateLimit => 0;

    public abstract void SendInvoke(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);
    public abstract void ReceiveInvoke(InvocationData reader);

    public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
    {
        NetPakWriter writer = NetMessages.GetServerWriter(this);
        SendInvoke(writer, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        ClientNetWrite.Send(this, writer);
    }
}

public abstract class ClientNetMethod<T1, T2, T3, T4, T5, T6, T7, T8> : INetMethod
{
    public NetMethodCaller AllowedCaller => NetMethodCaller.ClientCaller;

    public abstract uint NetMethodId {get;}

    public virtual float RateLimit => 0;

    public abstract void SendInvoke(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8);
    public abstract void ReceiveInvoke(InvocationData reader);

    public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
    {
        NetPakWriter writer = NetMessages.GetServerWriter(this);
        SendInvoke(writer, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        ClientNetWrite.Send(this, writer);
    }
}

public abstract class ClientNetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9> : INetMethod
{
    public NetMethodCaller AllowedCaller => NetMethodCaller.ClientCaller;

    public abstract uint NetMethodId {get;}

    public virtual float RateLimit => 0;

    public abstract void SendInvoke(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9);
    public abstract void ReceiveInvoke(InvocationData reader);

    public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
    {
        NetPakWriter writer = NetMessages.GetServerWriter(this);
        SendInvoke(writer, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
        ClientNetWrite.Send(this, writer);
    }
}

public abstract class ClientNetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : INetMethod
{
    public NetMethodCaller AllowedCaller => NetMethodCaller.ClientCaller;

    public abstract uint NetMethodId {get;}

    public virtual float RateLimit => 0;

    public abstract void SendInvoke(NetPakWriter writer, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10);
    public abstract void ReceiveInvoke(InvocationData reader);

    public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
    {
        NetPakWriter writer = NetMessages.GetServerWriter(this);
        SendInvoke(writer, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
        ClientNetWrite.Send(this, writer);
    }
}
