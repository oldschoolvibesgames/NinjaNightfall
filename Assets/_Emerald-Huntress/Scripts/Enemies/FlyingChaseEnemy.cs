using MoreMountains.CorgiEngine;
using UnityEngine;

public enum BehaviorType { Idle, Patrol }
public enum AttackType { ChasePlayer, DirectAttack, SweepAttack, DoubleDirectAttack }

public class FlyingChaseEnemy : MonoBehaviour
{
    [Header("Movement Settings")]
    public float chaseSpeed = 3f;
    public float ascendSpeed = 2f;
    public float visionRange = 8f;
    public float stopDistance = 1f;
    public float maxAscendHeight = 3f;
    public float horizontalWanderRange = 5f;
    public float hoverDelay = 1.5f;
    public float attackCancelTime = 3f;

    [Header("Behavior Settings")]
    public BehaviorType behavior = BehaviorType.Idle;
    public float patrolRange = 3f;
    public float patrolSpeed = 2f;

    [Header("Attack Abilities")]
    public bool enableChasePlayer = true;
    public bool enableDirectAttack = false;
    public bool enableSweepAttack = false;
    public bool enableDoubleDirectAttack = false;

    [Header("Projectile Settings")]
    public GameObject bulletPrefab;
    public Transform shootPoint;

    [Header("Visual")]
    public bool facePlayerWhenVisible = true;
    public SpriteRenderer spriteRenderer;

    private Transform _player;
    private bool _playerTarget = false;

    private Vector3 _startPosition;
    private Vector3 _targetAscendPosition;
    private Vector3 _patrolLeft;
    private Vector3 _patrolRight;
    private bool _movingRight = true;

    private Vector3 _directAttackTarget;
    private int _directAttackCount = 0;
    private int _doubleDirectCount = 0;
    private Vector3 _directAttackDirection;

    private enum State { Idle, Chasing, DirectAttack, SweepAttack, DoubleDirectAttack, Ascending, Hovering, Returning }
    private State _state = State.Idle;

    private float _hoverTimer;
    private float _attackTimer;

    private int _sweepPhase = 0;
    private int _sweepCycleCount = 0;
    private Vector3 _sweepTarget;
    private Vector3 _lastPlayerPosition;

    private void Awake()
    {
        _startPosition = transform.position;
        _patrolLeft = _startPosition + Vector3.left * patrolRange;
        _patrolRight = _startPosition + Vector3.right * patrolRange;
    }

    private void Update()
    {
        _attackTimer += Time.deltaTime;
        if (_attackTimer >= attackCancelTime && _state != State.Idle && _state != State.Hovering && _state != State.Returning && _state != State.Ascending)
        {
            _state = State.Hovering;
            _hoverTimer = hoverDelay;
            return;
        }

        UpdatePlayerReference();
        UpdatePlayerVision();

        switch (_state)
        {
            case State.Idle:
                HandleIdleOrPatrol();
                break;
            case State.Chasing:
                ChasePlayer();
                break;
            case State.DirectAttack:
                PerformDirectAttack();
                break;
            case State.SweepAttack:
                PerformSweepAttack();
                break;
            case State.DoubleDirectAttack:
                PerformDoubleDirectAttack();
                break;
            case State.Ascending:
                Ascend();
                break;
            case State.Hovering:
                Hover();
                break;
            case State.Returning:
                ReturnToStart();
                break;
        }
    }

    private void HandleIdleOrPatrol()
    {
        if (behavior == BehaviorType.Patrol)
        {
            Patrol();
        }
    }

    private void Patrol()
    {
        Vector3 target = _movingRight ? _patrolRight : _patrolLeft;
        Vector3 direction = (target - transform.position).normalized;
        transform.position += direction * patrolSpeed * Time.deltaTime;

        UpdateFacingDirection(direction.x);

        if (Vector2.Distance(transform.position, target) < 0.1f)
        {
            _movingRight = !_movingRight;
        }
    }

    private void UpdatePlayerReference()
    {
        if (_player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                _player = playerObj.transform;
            }
        }
    }

    private void UpdatePlayerVision()
    {
        if (_player == null)
        {
            _playerTarget = false;
            return;
        }

        float distance = Vector2.Distance(transform.position, _player.position);
        _playerTarget = distance <= visionRange;

        if (_playerTarget && (_state == State.Idle || _state == State.Returning))
        {
            AttackType selected = PickRandomAttack();
            _attackTimer = 0f;
            switch (selected)
            {
                case AttackType.ChasePlayer:
                    _state = State.Chasing;
                    break;
                case AttackType.DirectAttack:
                    _directAttackTarget = _player.position + (_player.position - transform.position).normalized * 2f;
                    _directAttackDirection = (_directAttackTarget - transform.position).normalized;
                    _directAttackCount = 0;
                    _state = State.DirectAttack;
                    break;
                case AttackType.SweepAttack:
                    StartSweepAttack();
                    break;
                case AttackType.DoubleDirectAttack:
                    _directAttackTarget = _player.position + (_player.position - transform.position).normalized * 2f;
                    _directAttackDirection = (_directAttackTarget - transform.position).normalized;
                    _doubleDirectCount = 0;
                    _state = State.DoubleDirectAttack;
                    break;
            }
        }
    }

    private AttackType PickRandomAttack()
    {
        var available = new System.Collections.Generic.List<AttackType>();

        if (enableChasePlayer) available.Add(AttackType.ChasePlayer);
        if (enableDirectAttack) available.Add(AttackType.DirectAttack);
        if (enableSweepAttack) available.Add(AttackType.SweepAttack);
        if (enableDoubleDirectAttack) available.Add(AttackType.DoubleDirectAttack);

        if (available.Count == 0) return AttackType.ChasePlayer;

        return available[Random.Range(0, available.Count)];
    }

    private void ChasePlayer()
    {
        if (_player == null)
        {
            _state = State.Idle;
            return;
        }

        Vector3 direction = (_player.position - transform.position).normalized;
        transform.position += direction * chaseSpeed * Time.deltaTime;

        UpdateFacingDirection(direction.x);

        if (Vector2.Distance(transform.position, _player.position) <= stopDistance)
        {
            StartAscend();
        }
    }

    private void PerformDirectAttack()
    {
        transform.position += _directAttackDirection * chaseSpeed * Time.deltaTime;
        UpdateFacingDirection(_directAttackDirection.x);

        if (Vector2.Distance(transform.position, _directAttackTarget) <= 0.1f)
        {
            _directAttackCount++;
            if (_directAttackCount < 2 && _playerTarget)
            {
                _directAttackTarget = _player.position + (_player.position - transform.position).normalized * 2f;
                _directAttackDirection = (_directAttackTarget - transform.position).normalized;
            }
            else
            {
                FireProjectile();
                StartAscend();
            }
        }
    }

    private void PerformDoubleDirectAttack()
    {
        transform.position += _directAttackDirection * chaseSpeed * Time.deltaTime;
        UpdateFacingDirection(_directAttackDirection.x);

        if (Vector2.Distance(transform.position, _directAttackTarget) <= 0.1f)
        {
            _doubleDirectCount++;
            if (_doubleDirectCount < 2 && _playerTarget)
            {
                _directAttackTarget = _player.position + (_player.position - transform.position).normalized * 2f;
                _directAttackDirection = (_directAttackTarget - transform.position).normalized;
            }
            else
            {
                FireProjectile();
                StartAscend();
            }
        }
    }

    private void FireProjectile()
    {
        if (_player == null || bulletPrefab == null || shootPoint == null) return;

        GameObject bullet = Instantiate(bulletPrefab, shootPoint.position, shootPoint.rotation);
        Vector3 direction = (_player.position - shootPoint.position).normalized;
        var projectile = bullet.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Direction = direction;
        }
    }

    private void StartSweepAttack()
    {
        _lastPlayerPosition = _player.position;
        _sweepPhase = 0;
        _sweepCycleCount = 0;
        _sweepTarget = _lastPlayerPosition;
        _state = State.SweepAttack;
        _attackTimer = 0f;
    }

    private void PerformSweepAttack()
    {
        Vector3 direction = (_sweepTarget - transform.position).normalized;
        transform.position += direction * chaseSpeed * Time.deltaTime;

        UpdateFacingDirection(direction.x);

        if (Vector2.Distance(transform.position, _sweepTarget) < 0.1f)
        {
            _sweepPhase++;

            switch (_sweepPhase)
            {
                case 1:
                    _sweepTarget = _lastPlayerPosition + new Vector3(0, maxAscendHeight, 0);
                    break;
                case 2:
                    _sweepTarget = new Vector3(
                        _lastPlayerPosition.x,
                        _lastPlayerPosition.y,
                        _lastPlayerPosition.z
                    );
                    break;
                case 3:
                    _sweepTarget = _lastPlayerPosition + new Vector3(maxAscendHeight, 0, 0);
                    break;
                case 4:
                case 6:
                    _sweepTarget = _lastPlayerPosition + new Vector3(-maxAscendHeight, 0, 0);
                    break;
                case 5:
                case 7:
                    _sweepTarget = _lastPlayerPosition + new Vector3(maxAscendHeight, 0, 0);
                    _sweepCycleCount++;
                    break;
                default:
                    StartAscend();
                    break;
            }
        }
    }

    private void StartAscend()
    {
        _targetAscendPosition = transform.position + new Vector3(
            Random.Range(-horizontalWanderRange, horizontalWanderRange),
            maxAscendHeight,
            0
        );
        _hoverTimer = hoverDelay;
        _attackTimer = 0f;
        _state = State.Ascending;
    }

    private void Ascend()
    {
        Vector3 direction = (_targetAscendPosition - transform.position).normalized;
        transform.position += direction * ascendSpeed * Time.deltaTime;

        UpdateFacingDirection(direction.x);

        if (Vector2.Distance(transform.position, _targetAscendPosition) < 0.1f)
        {
            _state = State.Hovering;
        }
    }

    private void Hover()
    {
        _hoverTimer -= Time.deltaTime;
        UpdateFacingDirection(0);

        if (_hoverTimer <= 0f)
        {
            AttackType selected = PickRandomAttack();
            _attackTimer = 0f;
            switch (selected)
            {
                case AttackType.ChasePlayer:
                    _state = _playerTarget ? State.Chasing : State.Returning;
                    break;
                case AttackType.DirectAttack:
                    if (_playerTarget)
                    {
                        _directAttackTarget = _player.position + (_player.position - transform.position).normalized * 2f;
                        _directAttackDirection = (_directAttackTarget - transform.position).normalized;
                        _directAttackCount = 0;
                        _state = State.DirectAttack;
                    }
                    else
                    {
                        _state = State.Returning;
                    }
                    break;
                case AttackType.SweepAttack:
                    if (_playerTarget)
                    {
                        StartSweepAttack();
                    }
                    else
                    {
                        _state = State.Returning;
                    }
                    break;
                case AttackType.DoubleDirectAttack:
                    if (_playerTarget)
                    {
                        _directAttackTarget = _player.position + (_player.position - transform.position).normalized * 2f;
                        _directAttackDirection = (_directAttackTarget - transform.position).normalized;
                        _doubleDirectCount = 0;
                        _state = State.DoubleDirectAttack;
                    }
                    else
                    {
                        _state = State.Returning;
                    }
                    break;
            }
        }
    }

    private void ReturnToStart()
    {
        Vector3 direction = (_startPosition - transform.position).normalized;
        transform.position += direction * chaseSpeed * Time.deltaTime;

        UpdateFacingDirection(direction.x);

        if (Vector2.Distance(transform.position, _startPosition) < 0.1f)
        {
            _state = State.Idle;
        }
    }

    private void UpdateFacingDirection(float moveDirectionX)
    {
        if (spriteRenderer == null) return;

        if (facePlayerWhenVisible && _player != null && _playerTarget)
        {
            float delta = _player.position.x - transform.position.x;
            spriteRenderer.flipX = delta < 0f;
        }
        else if (!Mathf.Approximately(moveDirectionX, 0f))
        {
            spriteRenderer.flipX = moveDirectionX < 0f;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        Gizmos.color = Color.green;
        Vector3 left = Application.isPlaying ? _patrolLeft : transform.position + Vector3.left * patrolRange;
        Vector3 right = Application.isPlaying ? _patrolRight : transform.position + Vector3.right * patrolRange;
        Gizmos.DrawLine(left, right);
    }
}
