using LocalMod.API.NetAbstractions;
using SDG.NetPak;
using SDG.Unturned;

namespace TestPlugin;

// ServerNetMethods are called by the server and sent to the client
public class ExampleRPC : ServerNetMethod<string>
{
    public const uint Id = 2234121;
    public override uint NetMethodId => Id;

    public override void ReceiveInvoke(ClientInvocationData data)
    {
        NetPakReader reader = data.Reader;
        reader.ReadString(out string message);
        UnturnedLog.info(message);
    }

    public override void SendInvoke(NetPakWriter writer, string message)
    {
        writer.WriteString(message);
    }
}
