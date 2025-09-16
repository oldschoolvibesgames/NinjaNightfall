using System.Collections;
using UnityEngine;
using MoreMountains.CorgiEngine;

public class MoveEffect : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer characterSprite;

    private Health _health;
    private CharacterHorizontalMovement _characterMovement;
    private float _originalWalkSpeed;
    private Color _originalColor;
    private Coroutine _currentEffect;

    private void Awake()
    {
        _health = GetComponent<Health>();
        if (_health != null)
        {
            _health.OnRevive += ResetEffect;
        }

        _characterMovement = GetComponent<CharacterHorizontalMovement>();
        if (_characterMovement != null)
        {
            _originalWalkSpeed = _characterMovement.WalkSpeed;
        }

        if (characterSprite != null)
        {
            _originalColor = characterSprite.color;
        }
    }

    public void ApplyMoveReduction(float duration, Color effectColor)
    {
        if (_characterMovement == null || characterSprite == null) return;

        if (_currentEffect != null)
        {
            StopCoroutine(_currentEffect);
        }

        _currentEffect = StartCoroutine(ApplyEffect(duration, effectColor));
    }

    private IEnumerator ApplyEffect(float duration, Color effectColor)
    {
        _characterMovement.WalkSpeed = _originalWalkSpeed / 2f;
        _characterMovement.ResetHorizontalSpeed();
        characterSprite.color = effectColor;

        yield return new WaitForSeconds(duration);

        ResetEffect();
        _currentEffect = null;
    }

    public void ResetEffect()
    {
        if (_currentEffect != null)
        {
            StopCoroutine(_currentEffect);
            _currentEffect = null;
        }

        if (_characterMovement != null)
        {
            _characterMovement.WalkSpeed = _originalWalkSpeed;
            _characterMovement.ResetHorizontalSpeed();
        }

        if (characterSprite != null)
        {
            characterSprite.color = _originalColor;
        }
    }

    private void OnDestroy()
    {
        if (_health != null)
        {
            _health.OnRevive -= ResetEffect;
        }
    }
}
