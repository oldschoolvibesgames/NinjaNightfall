using MoreMountains.CorgiEngine;
using UnityEngine;
using UnityEngine.Events;

public enum FacingDirection { Left, Right }

public class TowerEnemy : MonoBehaviour
{
    [Header("Configuração Inicial")]
    public FacingDirection initialFacing = FacingDirection.Right;

    [Header("Configurações de Ataque")]
    public float attackCooldown = 2f;
    public float viewDistance = 10f;
    public GameObject bulletPrefab;
    public Transform shootPoint;
    public LayerMask playerLayer;

    [Header("Referências")]
    public Animator animator;
    public UnityEvent onAttack;

    private float _attackTimer = 0f;
    private Transform _player;

    private void OnEnable()
    {
        float yRotation = (initialFacing == FacingDirection.Left) ? 180f : 0f;
        transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
    }

    private void Update()
    {
        if (_player == null)
        {
            DetectPlayer();
        }
        else
        {
            float distance = Vector2.Distance(transform.position, _player.position);
            if (distance > viewDistance)
            {
                _player = null;
                return;
            }

            RotateToFacePlayer();

            _attackTimer += Time.deltaTime;

            if (_attackTimer >= attackCooldown)
            {
                Attack();
                _attackTimer = 0f;
            }
        }
    }

    private void DetectPlayer()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, viewDistance, playerLayer);
        if (hit != null && hit.CompareTag("Player"))
        {
            _player = hit.transform;
        }
    }

    private void Attack()
    {
        animator.Play("Attack");
        onAttack?.Invoke();

        if (_player == null) return;

        var bullet = Instantiate(bulletPrefab, shootPoint.position, shootPoint.rotation);

        Vector3 directionToPlayer = (_player.position - shootPoint.position).normalized;

        var projectile = bullet.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Direction = directionToPlayer;
        }
    }


    private void RotateToFacePlayer()
    {
        if (_player == null) return;

        float yRotation = _player.position.x < transform.position.x ? 180f : 0f;
        transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, viewDistance);
    }

    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            float yRotation = (initialFacing == FacingDirection.Left) ? 180f : 0f;
            transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
        }
    }
}
