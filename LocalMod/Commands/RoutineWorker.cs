using System.Collections;
using UnityEngine;

namespace LocalMod.Commands;

internal class RoutineWorker : MonoBehaviour
{
    private IEnumerator? _Routine = null;

    public void StartRoutine(IEnumerator routine)
    {
        if (_Routine != null)
        {
            return;
        }

        _Routine = routine;

        StartCoroutine(routine);
    }

    private void Start()
    {
        DontDestroyOnLoad(this);
    }

    private void OnDestroy()
    {
        StopCoroutine(_Routine);
    }
}
