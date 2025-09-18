using System;
using UnityEngine;
using UnityEngine.Events;

public class InvokeByInput : MonoBehaviour
{
    public UnityEvent onBack;
    public UnityEvent onSelect;

    private PlayerInputs _inputs;

    private void Awake()
    {
        _inputs = new PlayerInputs();
        _inputs.Enable();
    }

    private void OnDestroy()
    {
        _inputs.Disable();
    }

    void Update()
    {
        if (_inputs.Menu.Back.triggered)
        {
            onBack?.Invoke();
        }
        else if (_inputs.Menu.Select.triggered)
        {
            onSelect?.Invoke();
        }
    }
}
