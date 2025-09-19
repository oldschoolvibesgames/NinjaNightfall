using UnityEngine;
using UnityEngine.Events;
using MoreMountains.CorgiEngine;

[RequireComponent(typeof(CharacterJump))]
public class PlayerLandingEvent : MonoBehaviour
{
    [Tooltip("Tempo mínimo no ar necessário para disparar o evento ao cair")]
    public float minAirTime = 0.5f;

    [Tooltip("Evento chamado quando o player cair no chão após estar no ar por tempo suficiente")]
    public UnityEvent OnHardLanding;

    private CorgiController _controller;
    private float _lastAirTime = 0f;

    private void Awake()
    {
        _controller = GetComponent<CorgiController>();
    }

    private void Update()
    {
        if (!_controller.State.IsGrounded)
        {
            _lastAirTime = _controller.TimeAirborne;
        }
        
        if (_controller.State.JustGotGrounded)
        {
            if (_lastAirTime >= minAirTime)
            {
                OnHardLanding?.Invoke();
            }
            
            _lastAirTime = 0f;
        }
    }
}