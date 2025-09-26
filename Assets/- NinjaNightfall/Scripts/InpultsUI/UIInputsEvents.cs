using UnityEngine;
using UnityEngine.InputSystem;

public class UIInputsEvents : MonoBehaviour
{
    [Header("Clips de Áudio")]
    public AudioClip moveUpClip;
    public AudioClip moveDownClip;
    public AudioClip moveLeftClip;
    public AudioClip moveRightClip;
    public AudioClip selectClip;
    public AudioClip backClip;

    private PlayerInputs _inputs;
    private AudioSFX _audioSFX;

    private void OnEnable()
    {
        _inputs = new PlayerInputs();
        _inputs.Menu.Enable();

        _inputs.Menu.Select.performed += OnSelect;
        _inputs.Menu.Back.performed += OnBack;
        _inputs.Menu.Move.performed += OnMovePerformed;

        _audioSFX = FindAnyObjectByType<AudioSFX>();
        if (_audioSFX == null)
        {
            Debug.LogWarning("AudioSFX não encontrado no objeto ou em seus pais.");
        }
    }

    private void OnDisable()
    {
        _inputs.Menu.Select.performed -= OnSelect;
        _inputs.Menu.Back.performed -= OnBack;
        _inputs.Menu.Move.performed -= OnMovePerformed;

        _inputs.Menu.Disable();
    }

    private void OnSelect(InputAction.CallbackContext ctx)
    {
        _audioSFX?.PlayAudio(selectClip);
    }

    private void OnBack(InputAction.CallbackContext ctx)
    {
        _audioSFX?.PlayAudio(backClip);
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();

        if (input.y > 0.5f)
        {
            _audioSFX?.PlayAudio(moveUpClip);
        }
        else if (input.y < -0.5f)
        {
            _audioSFX?.PlayAudio(moveDownClip);
        }
        else if (input.x < -0.5f)
        {
            _audioSFX?.PlayAudio(moveLeftClip);
        }
        else if (input.x > 0.5f)
        {
            _audioSFX?.PlayAudio(moveRightClip);
        }
    }
}