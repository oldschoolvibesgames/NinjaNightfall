using MoreMountains.CorgiEngine;
using UnityEngine;
using UnityEngine.Events;

public class WeaponBullet : MonoBehaviour
{
    [Header("Layers")]
    public LayerMask layersToCollider;
    public LayerMask targetDamage;

    [Header("Refs")]
    [SerializeField] private Rigidbody2D _rigidBody;
    [SerializeField] private Collider2D _collider2D;
    [SerializeField] private Animator _animator;
    [SerializeField] private SimpleEffectSpawner _effectSpawner;

    [Header("Area Damage")]
    [SerializeField] private bool isAreaDamage = false;
    [SerializeField] private float explosionRadius = 1.5f;

    [Header("Events")]
    public UnityEvent onShot;
    public UnityEvent onBounce;
    public UnityEvent onDestroy;
    
    private float _actualTime;
    private bool _countTime;
    private int _damage;
    private int _bounces;
    private float _shotForce;
    private int _weaponIndex;

    private SpriteRenderer _spriteRenderer;
    private Sprite _originalSprite;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer != null)
        {
            _originalSprite = _spriteRenderer.sprite;
        }

        if (_animator != null)
        {
            _animator.enabled = false;
        }
    }

    private void OnEnable()
    {
        ResetBulletState();
    }

    private void Update()
    {
        CountLifeTimer();
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        int colLayer = col.gameObject.layer;
        int layerMaskCollided = 1 << colLayer;

        if ((layersToCollider.value & layerMaskCollided) != 0)
        {
            if ((targetDamage.value & layerMaskCollided) != 0)
            {
                var health = col.gameObject.GetComponent<Health>();
                if (health != null)
                {
                    health.Damage(_damage, gameObject, 0.5f, 0f, transform.right);
                    if (health.CurrentHealth <= 0)
                    {
                        FindAnyObjectByType<KillsManager>().AddKillByWeapon(_weaponIndex);
                    }
                }
            }

            if (isAreaDamage)
            {
                DeactivateObject();
            }
            else
            {
                _bounces--;
                if (_bounces < 0)
                {
                    DeactivateObject();
                    return;
                }

                onBounce?.Invoke();

                Vector2 inDirection = _rigidBody.linearVelocity;
                Vector2 normal = col.contacts[0].normal;
                Vector2 reflectDir = Vector2.Reflect(inDirection, normal);

                _rigidBody.linearVelocity = reflectDir.normalized * _shotForce;

                float angle = Mathf.Atan2(reflectDir.y, reflectDir.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

                _effectSpawner.Spawn(transform.position, 0);
            }
        }
    }

    public void SetBullet(int newDamage, float newForce, float newLifeTime, int newBounces, int weaponIndex)
    {
        _damage = newDamage;
        _shotForce = newForce;
        _actualTime = newLifeTime;
        _bounces = newBounces;
        _weaponIndex = weaponIndex;

        Shot();
    }

    private void Shot()
    {
        onShot?.Invoke();
        _rigidBody.linearVelocity = Vector2.zero;
        _rigidBody.angularVelocity = 0;
        _rigidBody.AddForce(transform.right * _shotForce, ForceMode2D.Impulse);
        _countTime = true;
    }

    private void CountLifeTimer()
    {
        if (!_countTime) return;

        _actualTime -= Time.deltaTime;
        if (_actualTime <= 0f)
        {
            _countTime = false;
            DeactivateObject();
        }
    }

    private void DeactivateObject()
    {
        _countTime = false;

        onDestroy?.Invoke();
        _effectSpawner.Spawn(transform.position, 1);

        if (isAreaDamage)
        {
            Explode();
        }

        _collider2D.enabled = false;

        if (_animator != null)
        {
            _animator.enabled = true;
            _animator.Play("OnDestroy", 0, 0f);
        }

        Invoke(nameof(DisableGameObject), 0.5f);
    }

    private void DisableGameObject()
    {
        gameObject.SetActive(false);
    }

    private void Explode()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, targetDamage);
        foreach (var hit in hits)
        {
            var health = hit.GetComponent<Health>();
            if (health != null)
            {
                health.Damage(_damage, gameObject, 1f, 1f, (hit.transform.position - transform.position).normalized);
            }
        }
    }

    private void ResetBulletState()
    {
        _rigidBody.linearVelocity = Vector2.zero;
        _rigidBody.angularVelocity = 0f;
        _collider2D.enabled = true;
        _countTime = false;
        _actualTime = 0f;

        if (_spriteRenderer != null && _originalSprite != null)
        {
            _spriteRenderer.sprite = _originalSprite;
        }

        if (_animator != null)
        {
            _animator.enabled = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (isAreaDamage)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
}
