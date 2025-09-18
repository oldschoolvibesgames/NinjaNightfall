using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;

namespace MoreMountains.CorgiEngine
{
    [RequireComponent(typeof(CharacterHorizontalMovement))]
    [AddComponentMenu("Corgi Engine/Character/AI/Legacy/AI Shoot And Flee")]
    public class AIShootAndFlee : CorgiMonoBehaviour
    {
        public bool RespectPlatformEdges = true;
        public float EdgeDetectionDistance = 0.5f;
        public LayerMask GroundLayer;

        public float ViewDistance = 10f;
        public float AttackRange = 5f;
        public float FleeDuration = 2f;
        public GameObject ProjectilePrefab;
        public Transform ShootSpawnPoint;
        public int InitialPoolSize = 5;
        public float PostAttackCooldown = 1f;
        public LayerMask ObstacleLayer;

        protected CorgiController _controller;
        protected Character _character;
        protected Health _health;
        protected CharacterHorizontalMovement _characterHorizontalMovement;
        protected Vector2 _startPosition;
        protected Vector2 _direction;
        protected GameObject _player;
        protected bool _isFleeing = false;
        protected float _fleeStartTime;
        protected List<GameObject> _projectilePool = new List<GameObject>();
        protected Animator _animator;
        protected bool _inAttackRange;
        protected bool _isAttacking = false;
        protected float _lastAttackTime = -Mathf.Infinity;
        protected Vector3 _initialShootSpawnLocalPosition;

        protected virtual void Start()
        {
            Initialization();
            InitializeProjectilePool();
        }

        protected virtual void Initialization()
        {
            _controller = GetComponentInParent<CorgiController>();
            _character = GetComponentInParent<Character>();
            _characterHorizontalMovement = _character?.FindAbility<CharacterHorizontalMovement>();
            _health = GetComponent<Health>();
            _startPosition = transform.position;
            _direction = _character.IsFacingRight ? Vector2.right : Vector2.left;
            _animator = GetComponentInChildren<Animator>();
            _initialShootSpawnLocalPosition = ShootSpawnPoint.localPosition;
        }

        protected virtual void Update()
        {
            if (_character == null || _character.ConditionState.CurrentState == CharacterStates.CharacterConditions.Dead || _character.ConditionState.CurrentState == CharacterStates.CharacterConditions.Frozen)
            {
                return;
            }

            if (_player == null)
            {
                _player = GameObject.FindGameObjectWithTag("Player");
                if (_player == null) return;
            }

            _inAttackRange = IsPlayerVisible(AttackRange);

            if (_isFleeing)
            {
                if (Time.time - _fleeStartTime > FleeDuration)
                {
                    _isFleeing = false;
                }
                else
                {
                    _characterHorizontalMovement.SetHorizontalMove(_direction.x);
                    return;
                }
            }

            if (!_isAttacking)
            {
                PatrolOrChase();
            }
        }

        protected virtual void PatrolOrChase()
        {
            if (_player == null) return;

            if (IsPlayerVisible(ViewDistance))
            {
                bool playerRight = _player.transform.position.x > transform.position.x;
                if (!_isAttacking && (_character.IsFacingRight != playerRight))
                {
                    _character.Flip(playerRight);
                    _direction = playerRight ? Vector2.right : Vector2.left;
                }

                if (_inAttackRange)
                {
                    _characterHorizontalMovement.SetHorizontalMove(0f);

                    if (Time.time >= _lastAttackTime + PostAttackCooldown && !_isAttacking && _animator != null)
                    {
                        _isAttacking = true;
                        _animator.ResetTrigger("Attack");
                        _animator.SetTrigger("Attack");
                        StartCoroutine(WaitForAttackToComplete());
                    }
                    return;
                }
            }

            CheckForWalls();
            _characterHorizontalMovement.SetHorizontalMove(_direction.x);
        }

        protected virtual bool IsPlayerVisible(float range)
        {
            Vector2 origin = transform.position;
            Vector2 direction = _character.IsFacingRight ? Vector2.right : Vector2.left;
            RaycastHit2D hit = Physics2D.Raycast(origin, direction, range, ObstacleLayer);

            if (hit.collider != null && hit.collider.gameObject.CompareTag("Player"))
            {
                return true;
            }

            return false;
        }

        protected virtual IEnumerator WaitForAttackToComplete()
        {
            yield return null;
            yield return new WaitUntil(() => _animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"));
            yield return new WaitUntil(() => !_animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"));
            _lastAttackTime = Time.time;
            _isAttacking = false;
        }

        protected virtual void CheckForWalls()
        {
            bool flip = false;

            if ((_direction.x < 0 && _controller.State.IsCollidingLeft) || (_direction.x > 0 && _controller.State.IsCollidingRight))
            {
                flip = true;
            }

            if (RespectPlatformEdges && !IsGroundAhead())
            {
                flip = true;
            }

            if (flip)
            {
                ChangeDirection();
            }
        }

        protected virtual bool IsGroundAhead()
        {
            Vector2 origin = transform.position + new Vector3(_direction.x * 0.5f, 0f);
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, EdgeDetectionDistance, GroundLayer);
            return hit.collider != null;
        }

        protected virtual void ChangeDirection()
        {
            _direction = -_direction;
        }

        public virtual void TriggerFlee()
        {
            _isFleeing = true;
            _fleeStartTime = Time.time;
            _direction = _character.IsFacingRight ? Vector2.left : Vector2.right;
        }

        protected virtual void InitializeProjectilePool()
        {
            for (int i = 0; i < InitialPoolSize; i++)
            {
                GameObject proj = Instantiate(ProjectilePrefab);
                proj.SetActive(false);
                SetupProjectileEvents(proj);
                _projectilePool.Add(proj);
            }
        }

        protected virtual GameObject GetPooledProjectile()
        {
            foreach (GameObject proj in _projectilePool)
            {
                if (!proj.activeInHierarchy)
                {
                    return proj;
                }
            }

            GameObject newProj = Instantiate(ProjectilePrefab);
            newProj.SetActive(false);
            SetupProjectileEvents(newProj);
            _projectilePool.Add(newProj);
            return newProj;
        }

        protected virtual void SetupProjectileEvents(GameObject projectile)
        {
            TriggerActionOnHit trigger = projectile.GetComponent<TriggerActionOnHit>();
            if (trigger != null)
            {
                trigger.OnValidHit -= OnProjectileHit;
                trigger.OnValidHit += OnProjectileHit;
            }
        }

        protected virtual void OnProjectileHit()
        {
            if (_player != null && Vector2.Distance(_player.transform.position, transform.position) <= AttackRange)
            {
                TriggerFlee();
            }
        }

        public virtual void Shoot()
        {
            Vector3 adjustedSpawnPosition = _initialShootSpawnLocalPosition;
            adjustedSpawnPosition.x *= _character.IsFacingRight ? 1 : -1;
            ShootSpawnPoint.localPosition = adjustedSpawnPosition;

            GameObject projectile = GetPooledProjectile();
            projectile.transform.position = ShootSpawnPoint.position;
            projectile.transform.rotation = ShootSpawnPoint.rotation;

            SetupProjectileEvents(projectile);
            projectile.SetActive(true);

            Projectile projComponent = projectile.GetComponent<Projectile>();
            if (projComponent != null)
            {
                Vector3 shootDirection = _character.IsFacingRight ? Vector3.right : Vector3.left;
                projComponent.SetDirection(shootDirection, Quaternion.identity, _character.IsFacingRight);
                projComponent.SetOwner(this.gameObject);
            }
        }

        protected virtual void OnDrawGizmosSelected()
        {
            Character character = GetComponentInParent<Character>();
            if (character != null)
            {
                Vector3 dir = character.IsFacingRight ? Vector3.right : Vector3.left;

                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, transform.position + dir * ViewDistance);

                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, transform.position + dir * AttackRange);
            }

            // Gizmo laranja para borda de plataforma
            if (RespectPlatformEdges)
            {
                Vector2 gizmoDirection = Application.isPlaying ? _direction : Vector2.right;
                Vector2 origin = (Vector2)transform.position + new Vector2(gizmoDirection.x * 0.5f, 0f);
                Gizmos.color = new Color(1f, 0.5f, 0f); // Laranja
                Gizmos.DrawLine(origin, origin + Vector2.down * EdgeDetectionDistance);
            }
        }
        
        public void ResetEnemyState()
        {
            _isFleeing = false;
            _isAttacking = false;
            _lastAttackTime = -Mathf.Infinity;
            _player = null;
            _direction = _character.IsFacingRight ? Vector2.right : Vector2.left;
            _characterHorizontalMovement.SetHorizontalMove(0f);
            if (_animator != null)
            {
                _animator.ResetTrigger("Attack");
            }
            //transform.position = _startPosition;
        }



    }
}
