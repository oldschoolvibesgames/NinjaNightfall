using System.Collections;
using UnityEngine;
using MoreMountains.CorgiEngine;

public class JumpEffect : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer characterSprite;

    private Health _health;
    private CharacterJump _characterJump;
    private float _originalJumpHeight;
    private Color _originalColor;
    private Coroutine _currentEffect;

    private void Awake()
    {
        _health = GetComponent<Health>();
        _health.OnRevive += ResetEffect;
        
        _characterJump = GetComponent<CharacterJump>();

        if (_characterJump != null)
        {
            _originalJumpHeight = _characterJump.JumpHeight;
        }

        if (characterSprite != null)
        {
            _originalColor = characterSprite.color;
        }
    }
    
    private void OnDestroy()
    {
        if (_health != null)
        {
            _health.OnRevive -= ResetEffect;
        }
    }


    public void ApplyJumpReduction(float duration, Color effectColor)
    {
        if (_characterJump == null || characterSprite == null) return;

        if (_currentEffect != null)
        {
            StopCoroutine(_currentEffect);
        }

        _currentEffect = StartCoroutine(ApplyEffect(duration, effectColor));
    }

    private IEnumerator ApplyEffect(float duration, Color effectColor)
    {
        _characterJump.JumpHeight = _originalJumpHeight / 2f;
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

        if (_characterJump != null)
        {
            _characterJump.JumpHeight = _originalJumpHeight;
        }

        if (characterSprite != null)
        {
            characterSprite.color = _originalColor;
        }
    }

}