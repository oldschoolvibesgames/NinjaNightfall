using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UISelectionLevels : MonoBehaviour
{
    public Button[] buttonsLevel;
    private int _index;
    private EventSystem _eventSystem;
    private PlayerInputs _inputActions;

    private float _lastInputTime;
    private float _inputCooldown = 0.3f;
    private bool doOnce;

    private void Awake()
    {
        _eventSystem = EventSystem.current;
        _inputActions = new PlayerInputs();
        _inputActions.Menu.Enable();
    }

    private void OnEnable()
    {
        /*_index = 0;
        SkipToNextInteractable(1);
        SetFocus();*/
        doOnce = false;
    }

    private void OnDestroy()
    {
        _inputActions.Disable();
    }

    private void Update()
    {
        if (!doOnce)
        {
            _index = 0;
            SkipToNextInteractable(1);
            SetFocus();
            doOnce = true;
        }
        
        Vector2 moveInput = _inputActions.Menu.Move.ReadValue<Vector2>();

        if (Time.time - _lastInputTime >= _inputCooldown)
        {
            if (moveInput.x > 0.5f)
            {
                SkipToNextInteractable(1); // direita
                SetFocus();
                _lastInputTime = Time.time;
            }
            else if (moveInput.x < -0.5f)
            {
                SkipToNextInteractable(-1); // esquerda
                SetFocus();
                _lastInputTime = Time.time;
            }
        }
    }

    private void SkipToNextInteractable(int direction)
    {
        int startIndex = _index;

        do
        {
            _index += direction;

            if (_index >= buttonsLevel.Length)
                _index = 0;
            else if (_index < 0)
                _index = buttonsLevel.Length - 1;

            if (buttonsLevel[_index].gameObject.activeInHierarchy)
                return;

        } while (_index != startIndex);
    }

    private void SetFocus()
    {
        //if(buttonsLevel[_index].gameObject.activeInHierarchy)
        _eventSystem.SetSelectedGameObject(buttonsLevel[_index].gameObject);

        foreach (var i in buttonsLevel)
        {
            if (i.gameObject != buttonsLevel[_index].gameObject)
            {
               i.GetComponent<FocusEventButton>().EnableIcon(false);
            }
        }
        buttonsLevel[_index].GetComponent<FocusEventButton>().EnableIcon(true);
    }
}