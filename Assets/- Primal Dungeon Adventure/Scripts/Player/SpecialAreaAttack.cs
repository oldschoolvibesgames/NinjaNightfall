using System;
using MoreMountains.CorgiEngine;
using UnityEngine;
using UnityEngine.Events;

public class SpecialAreaAttack : MonoBehaviour
{
    public float attackRadius = 3f;
    public float areaDuration = 2f;
    public LayerMask enemyLayer;
    public string bossTag = "Boss";

    public UnityEvent onAttack;
    
    [Header("Estado do Especial")]
    public bool unlocked = false;
    public bool isReady = false;
    public bool infinite = false;
    private int _killCount = 0;
    private int _requiredKills = 10;
    private bool _isActive = false;
    private float _timer = 0f;
    private SpecialUI _specialUI;

    [Header("VFX")]
    public GameObject areaEffectObject;
    public GameObject specialReadyVfx;

    private PlayerInputs _playerInputs;
    private GamePause _gamePause;

    private void Awake()
    {
        _playerInputs = new PlayerInputs();
        _playerInputs.Gameplay.Enable();

        _gamePause = FindAnyObjectByType<GamePause>();

        if (_specialUI == null) _specialUI = FindAnyObjectByType<SpecialUI>();
        UpdateUI();

        if (areaEffectObject != null)
            areaEffectObject.SetActive(false);
    }

    private void OnDestroy()
    {
        _playerInputs.Disable();
        _playerInputs.Gameplay.Disable();
    }

    private void Update()
    {
        if(_gamePause.IsPaused()) return;
        
        if (_playerInputs.Gameplay.Special.triggered)
        {
            TriggerSpecialAttack();
        }

        if (_isActive)
        {
            _timer -= Time.deltaTime;
            ApplyAreaDamage();

            if (_timer <= 0f)
            {
                _isActive = false;
                if (areaEffectObject != null)
                    areaEffectObject.SetActive(false);
            }
        }

        _specialUI.IsUnlocked(unlocked);
        
        if(isReady && unlocked && !specialReadyVfx.activeSelf) specialReadyVfx.SetActive(true);
        else if(!isReady && specialReadyVfx.activeSelf) specialReadyVfx.SetActive(false);
    }

    public void RegisterKill()
    {
        if (!unlocked) return;

        _killCount++;
        if (_killCount >= _requiredKills)
        {
            isReady = true;
        }

        UpdateUI();
    }

    public void TriggerSpecialAttack()
    {
        if ((!isReady && !infinite) || !unlocked) return;

        if (!infinite)
        {
            isReady = false;
            _killCount = 0;
        }

        onAttack?.Invoke();
        _isActive = true;
        _timer = areaDuration;

        if (areaEffectObject != null)
            areaEffectObject.SetActive(true);

        UpdateUI();
    }

    private void ApplyAreaDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRadius, enemyLayer);
        foreach (var hit in hits)
        {
            if (!hit.CompareTag(bossTag))
            {
                var health = hit.GetComponent<Health>();
                if (health != null)
                {
                    health.Damage(9999, this.gameObject, 1f, 1f, this.transform.right);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }

    public void UnlockSpecial()
    {
        unlocked = true;
        _killCount = 0;
        isReady = false;
    }

    private void UpdateUI()
    {
        _specialUI.SetFilled(infinite ? 1f : (float)((float)_killCount / (float)_requiredKills));
    }
}
