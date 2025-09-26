using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class FocusEventButton : MonoBehaviour
{
    [Header("Focus Events")]
    public UnityEvent onFocusEnter;
    public UnityEvent onFocusExit;

    private bool _hasFocus = false;

    private void Update()
    {
        GameObject current = EventSystem.current.currentSelectedGameObject;

        if (current == gameObject && !_hasFocus)
        {
            _hasFocus = true;
            onFocusEnter.Invoke();
        }
        else if (current != gameObject && _hasFocus)
        {
            _hasFocus = false;
            onFocusExit.Invoke();
        }
    }
}