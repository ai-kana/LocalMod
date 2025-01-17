using Cysharp.Threading.Tasks;
using LocalMod.API.NetAbstractions;
using SDG.NetPak;
using SDG.Unturned;

namespace TestPlugin;

// ServerNetMethods are called by the server and sent to the client
public class ExampleRPC : ServerNetMethod<float>
{
    public async UniTask DoFlip(float delta)
    {
        if (delta == 0)
        {
            return;
        }

        await UniTask.Yield();
        Player self = Player.player ?? throw new("Player invalid");
        if (delta > 0f)
        {
            self.gameObject.transform.localScale *= delta;
            return;
        }

            self.gameObject.transform.localScale /= (delta * -1);
    }

    public override void ReceiveInvoke(ClientInvocationData data)
    {
        NetPakReader reader = data.Reader;
        reader.ReadFloat(out float delta);
        DoFlip(delta).Forget();
    }

    public override void SendInvoke(NetPakWriter writer, float delta)
    {
        writer.WriteFloat(delta);
    }
}
