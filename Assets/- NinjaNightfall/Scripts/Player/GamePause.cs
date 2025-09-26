using System;
using MoreMountains.CorgiEngine;
using UnityEngine;

public class GamePause : MonoBehaviour
{
    [SerializeField] private GameObject _pauseMenu;
    private float _delay = 0.5f;
    private float _actualDelay;
    private bool _lastStatusPaused;
    private bool _countTime;
    private PlayerInputs _playerInputs;
    private PauseButton _pauseButton;

    private void Awake()
    {
        _pauseMenu.SetActive(false);
        _playerInputs = new PlayerInputs();
        _playerInputs.Gameplay.Enable();
        _pauseButton = FindAnyObjectByType<PauseButton>();
    }

    private void OnDestroy()
    {
        _playerInputs.Gameplay.Disable();
        _playerInputs.Disable();
        Time.timeScale = 1;
    }

    private void Update()
    {
        if (_playerInputs.Gameplay.Pause.triggered)
        {
            _pauseButton.PauseButtonAction();
        }
        
        if (_countTime)
        {
            if (_actualDelay > 0) _actualDelay -= Time.deltaTime;
            else
            {
                _countTime = false;
                _lastStatusPaused = _pauseMenu.activeSelf;
            }
        }
        else
        {
            if (_lastStatusPaused != _pauseMenu.activeSelf)
            {
                _actualDelay = _delay;
                _countTime = true;
            }
        }
    }

    public bool IsPaused()
    {
        if (_pauseMenu.activeSelf)
        {
            return true;
        }
        
        return _lastStatusPaused;
    }
}
