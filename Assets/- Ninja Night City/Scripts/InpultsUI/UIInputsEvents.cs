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

    [Header("Config")]
    [Range(0f, 1f)] public float directionThreshold = 0.5f;

    private PlayerInputs _inputs;
    private AudioSFX _audioSFX;

    private bool _upHeld;
    private bool _downHeld;
    private bool _leftHeld;
    private bool _rightHeld;

    private void OnEnable()
    {
        _inputs = new PlayerInputs();
        _inputs.Menu.Enable();

        _inputs.Menu.Select.performed += OnSelect;
        _inputs.Menu.Back.performed += OnBack;

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

        _inputs.Menu.Disable();
    }

    private void Update()
    {
        Vector2 input = _inputs.Menu.Move.ReadValue<Vector2>();

        bool up = input.y > directionThreshold;
        bool down = input.y < -directionThreshold;
        bool left = input.x < -directionThreshold;
        bool right = input.x > directionThreshold;

        if (up && !_upHeld)
        {
            _audioSFX?.PlayAudio(moveUpClip);
            _upHeld = true;
        }
        else if (!up)
        {
            _upHeld = false;
        }

        if (down && !_downHeld)
        {
            _audioSFX?.PlayAudio(moveDownClip);
            _downHeld = true;
        }
        else if (!down)
        {
            _downHeld = false;
        }

        if (left && !_leftHeld)
        {
            _audioSFX?.PlayAudio(moveLeftClip);
            _leftHeld = true;
        }
        else if (!left)
        {
            _leftHeld = false;
        }

        if (right && !_rightHeld)
        {
            _audioSFX?.PlayAudio(moveRightClip);
            _rightHeld = true;
        }
        else if (!right)
        {
            _rightHeld = false;
        }
    }

    private void OnSelect(InputAction.CallbackContext ctx)
    {
        _audioSFX?.PlayAudio(selectClip);
    }

    private void OnBack(InputAction.CallbackContext ctx)
    {
        _audioSFX?.PlayAudio(backClip);
    }
}
