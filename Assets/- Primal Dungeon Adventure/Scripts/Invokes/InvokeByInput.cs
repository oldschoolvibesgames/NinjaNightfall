using System;
using UnityEngine;
using UnityEngine.Events;

public class InvokeByInput : MonoBehaviour
{
    public UnityEvent onBack;
    public UnityEvent onSelect;
    public UnityEvent onUp;
    public UnityEvent onDown;

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
        Vector2 moveInput = _inputs.Menu.Move.ReadValue<Vector2>();

        if (_inputs.Menu.Back.triggered)
        {
            onBack?.Invoke();
        }
        else if (_inputs.Menu.Select.triggered)
        {
            onSelect?.Invoke();
        }
        else if (moveInput.y > 0.5f)
        {
            onUp?.Invoke();
        }
        else if (moveInput.y < -0.5f)
        {
            onDown?.Invoke();
        }
    }

}
