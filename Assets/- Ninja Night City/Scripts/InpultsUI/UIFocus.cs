using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIFocus : MonoBehaviour
{
    public GameObject firstFocus;

    private EventSystem _eventSystem;
    private PlayerInputs _inputActions;
    private GameObject _lastSelection;

    private void Awake()
    {
        _eventSystem = EventSystem.current;
        _inputActions = new PlayerInputs();
        _inputActions.Enable();
    }

    private void Update()
    {
        if (_eventSystem.currentSelectedGameObject != _lastSelection)
        {
            _lastSelection = _eventSystem.currentSelectedGameObject;
            // Optional: play SFX on selection change
        }
    }

    private void OnDestroy()
    {
        _inputActions.Disable();
        _inputActions.Gameplay.Disable();
    }

    private void OnEnable()
    {
        _inputActions.Menu.Move.performed += OnMovePerformed;
        SetFocusIfNeeded();
    }

    private void OnDisable()
    {
        _inputActions.Menu.Move.performed -= OnMovePerformed;
        _inputActions.Menu.Disable();
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        if (_eventSystem.currentSelectedGameObject == null || !_eventSystem.currentSelectedGameObject.activeInHierarchy)
        {
            SetFocusIfNeeded();
        }
    }

    private void SetFocusIfNeeded()
    {
        if (firstFocus != null && firstFocus.activeInHierarchy)
        {
            _eventSystem.SetSelectedGameObject(firstFocus);
        }
        else
        {
            Selectable[] allSelectables = FindObjectsOfType<Selectable>(false);
            foreach (var selectable in allSelectables)
            {
                if (selectable.gameObject.activeSelf && selectable.IsInteractable())
                {
                    _eventSystem.SetSelectedGameObject(selectable.gameObject);
                    break;
                }
            }
        }
    }
}