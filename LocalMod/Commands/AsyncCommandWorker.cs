using Cysharp.Threading.Tasks;

namespace LocalMod.Commands;

public class AsyncCommandWorker
{
    private readonly Queue<IAsyncCommand> _Commands;
    public AsyncCommandWorker(Queue<IAsyncCommand> commands)
    {
        _Commands = commands;
    }

    public async UniTask<int> ExecuteAsync()
    {
        int i = 0;
        while (_Commands.TryDequeue(out IAsyncCommand command))
        {
            await command.ExecuteAsync();
            i++;
        }

        return i;
    }
}
