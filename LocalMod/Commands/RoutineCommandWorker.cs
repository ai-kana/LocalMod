using System.Collections;
using UnityEngine;

namespace LocalMod.Commands;

public class RoutineCommandWorker
{
    private readonly Queue<ICommand> _Commands;
    private readonly GameObject _Runner;
    private readonly RoutineWorker _Worker;

    public RoutineCommandWorker(Queue<ICommand> commands)
    {
        _Commands = commands;
        _Runner = new($"{this.GetType()}");
        _Worker = _Runner.AddComponent<RoutineWorker>();
    }

    private IEnumerator Routine()
    {
        WaitForEndOfFrame waiter = new();

        while (_Commands.TryDequeue(out ICommand command))
        {
            command.Execute();
            yield return waiter;
        }

        GameObject.Destroy(_Runner);
    }

    public void Execute()
    {
        _Worker.StartRoutine(Routine());
    }
}
