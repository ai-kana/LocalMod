using Cysharp.Threading.Tasks;

namespace LocalMod.Commands;

public interface ICommand
{
    public UniTask Execute();
}
