using UnityEngine.Events;
using UnityEngine;

public class InvokeByAnyKey : MonoBehaviour
{
    public UnityEvent onAnyKey;
    public bool doOnce;
    private bool _done;

    private void Update()
    {
        if (Input.anyKey)
        {
            onAnyKey?.Invoke();
        }
    }
}
