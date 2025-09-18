using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;

namespace MoreMountains.CorgiEngine
{
    [RequireComponent(typeof(CharacterHorizontalMovement))]
    [AddComponentMenu("Corgi Engine/Character/AI/Legacy/AI Jump Patrol And Attack")]
    public class AIJumpPatrolAndAttack : CorgiMonoBehaviour
    {
        public float ViewDistance = 10f;
        public float AttackRange = 5f;
        public float WaitAfterAttack = 1f;
        public float JumpForce = 10f;
        public float JumpRayLength = 2f;
        public float PlatformDetectionRange = 5f;
        public float PatrolRange = 10f;
        public float VerticalPlayerJumpThreshold = 1.5f;
        public LayerMask PlatformLayer;
        public float WallDetectionDistance = 0.6f;
        public float VerticalDropTolerance = 0.5f;

        public GameObject ProjectilePrefab;
        public Transform ShootSpawnPoint;
        public int InitialPoolSize = 5;

        protected CorgiController _controller;
        protected Character _character;
        protected CharacterHorizontalMovement _characterHorizontalMovement;
        protected Animator _animator;
        protected GameObject _player;
        protected Vector2 _startPosition;
        protected Vector2 _direction;
        protected float _lastAttackTime = -Mathf.Infinity;
        protected bool _isAttacking = false;
        protected bool _isWaitingAfterAttack = false;
        protected float _waitStartTime;
        protected bool _isDroppingToPlayer = false;

        protected List<GameObject> _projectilePool = new List<GameObject>();
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
            _animator = GetComponentInChildren<Animator>();
            _startPosition = transform.position;
            _direction = _character.IsFacingRight ? Vector2.right : Vector2.left;
            _initialShootSpawnLocalPosition = ShootSpawnPoint.localPosition;
        }

        protected virtual void Update()
        {
            if (_character == null ||
                _character.ConditionState.CurrentState == CharacterStates.CharacterConditions.Dead ||
                _character.ConditionState.CurrentState == CharacterStates.CharacterConditions.Frozen)
            {
                return;
            }

            if (_player == null)
            {
                _player = GameObject.FindGameObjectWithTag("Player");
                if (_player == null) return;
            }

            float distanceToPlayer = Vector2.Distance(_player.transform.position, transform.position);
            bool inView = distanceToPlayer <= ViewDistance;
            bool inRange = distanceToPlayer <= AttackRange;

            float yDiff = _player.transform.position.y - transform.position.y;

            if (_isWaitingAfterAttack)
            {
                if (Time.time - _waitStartTime >= WaitAfterAttack)
                {
                    _isWaitingAfterAttack = false;
                }
                else
                {
                    _characterHorizontalMovement.SetHorizontalMove(0f);
                    return;
                }
            }

            if (_isAttacking)
            {
                _characterHorizontalMovement.SetHorizontalMove(0f);
                return;
            }

            if (_isDroppingToPlayer)
            {
                if (yDiff > 0)
                {
                    _isDroppingToPlayer = false;
                }
                else
                {
                    _characterHorizontalMovement.SetHorizontalMove(_direction.x);

                    if (Mathf.Abs(yDiff) < VerticalPlayerJumpThreshold && _controller.State.IsGrounded)
                    {
                        _isDroppingToPlayer = false;
                    }
                    return;
                }
            }

            if (inView)
            {
                FacePlayer();

                if (inRange)
                {
                    if (yDiff < -VerticalDropTolerance)
                    {
                        _isDroppingToPlayer = true;
                        _direction = (_player.transform.position.x > transform.position.x) ? Vector2.right : Vector2.left;
                        return;
                    }
                    else if (yDiff > VerticalPlayerJumpThreshold)
                    {
                        TryJumpToPlatform(true);
                        return;
                    }

                    _characterHorizontalMovement.SetHorizontalMove(0f);
                    Attack();
                    return;
                }
                else
                {
                    MoveTowardsPlayer();
                }
            }
            else
            {
                Patrol();
            }
        }

        protected virtual void Attack()
        {
            if (_animator != null)
            {
                _isAttacking = true;
                _animator.ResetTrigger("Attack");
                _animator.SetTrigger("Attack");
                StartCoroutine(WaitForAttackEnd());
            }
        }

        protected virtual IEnumerator WaitForAttackEnd()
        {
            yield return null;
            yield return new WaitUntil(() => _animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"));
            yield return new WaitUntil(() => !_animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"));
            _lastAttackTime = Time.time;
            _isAttacking = false;
            _isWaitingAfterAttack = true;
        }

        public virtual void Shoot()
        {
            Vector3 adjustedSpawnPosition = _initialShootSpawnLocalPosition;
            adjustedSpawnPosition.x *= _character.IsFacingRight ? 1 : -1;
            ShootSpawnPoint.localPosition = adjustedSpawnPosition;

            GameObject projectile = GetPooledProjectile();
            projectile.transform.position = ShootSpawnPoint.position;
            projectile.transform.rotation = ShootSpawnPoint.rotation;

            projectile.SetActive(true);

            Projectile projComponent = projectile.GetComponent<Projectile>();
            if (projComponent != null)
            {
                Vector3 shootDirection = _character.IsFacingRight ? Vector3.right : Vector3.left;
                projComponent.SetDirection(shootDirection, Quaternion.identity, _character.IsFacingRight);
                projComponent.SetOwner(this.gameObject);
            }
        }

        protected virtual void InitializeProjectilePool()
        {
            for (int i = 0; i < InitialPoolSize; i++)
            {
                GameObject proj = Instantiate(ProjectilePrefab);
                proj.SetActive(false);
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
            _projectilePool.Add(newProj);
            return newProj;
        }

        protected virtual void MoveTowardsPlayer()
        {
            float yDiff = _player.transform.position.y - transform.position.y;

            if (yDiff < -VerticalDropTolerance)
            {
                _isDroppingToPlayer = true;
                _direction = (_player.transform.position.x > transform.position.x) ? Vector2.right : Vector2.left;
                return;
            }

            Vector2 direction = (_player.transform.position.x > transform.position.x) ? Vector2.right : Vector2.left;
            _characterHorizontalMovement.SetHorizontalMove(direction.x);

            if (_character.IsFacingRight != (_player.transform.position.x > transform.position.x))
            {
                _character.Flip(_player.transform.position.x > transform.position.x);
            }

            TryJumpToPlatform(true);
        }

        protected virtual void Patrol()
        {
            float patrolOffset = transform.position.x - _startPosition.x;
            if (Mathf.Abs(patrolOffset) > PatrolRange)
            {
                _direction = (_startPosition.x - transform.position.x > 0) ? Vector2.right : Vector2.left;
            }

            _characterHorizontalMovement.SetHorizontalMove(_direction.x);

            if ((_direction.x < 0 && _controller.State.IsCollidingLeft) ||
                (_direction.x > 0 && _controller.State.IsCollidingRight))
            {
                _direction = -_direction;
            }

            TryJumpToPlatform(false);
        }

        protected virtual void TryJumpToPlatform(bool prioritizeVertical)
        {
            if (!_controller.State.IsGrounded) return;

            Vector2 origin = _controller.transform.position;
            Vector2 forward = _character.IsFacingRight ? Vector2.right : Vector2.left;
            Vector2 diagForward = forward + Vector2.up;

            RaycastHit2D wallHit = Physics2D.Raycast(origin, forward, WallDetectionDistance, PlatformLayer);
            RaycastHit2D diagUpHit = Physics2D.Raycast(origin, diagForward.normalized, PlatformDetectionRange, PlatformLayer);
            RaycastHit2D upHit = Physics2D.Raycast(origin, Vector2.up, JumpRayLength, PlatformLayer);

            if (wallHit.collider != null || (prioritizeVertical && upHit.collider != null) || diagUpHit.collider != null)
            {
                DoJump();
            }
        }

        protected virtual void DoJump()
        {
            _controller.SetVerticalForce(JumpForce);
            if (_animator != null)
            {
                _animator.SetBool("Jumping", true);
            }
            StartCoroutine(ResetJumpAnimation());
        }

        protected virtual IEnumerator ResetJumpAnimation()
        {
            yield return new WaitForSeconds(0.3f);
            if (_animator != null)
            {
                _animator.SetBool("Jumping", false);
            }
        }

        protected virtual void FacePlayer()
        {
            if (_character == null || _player == null) return;
            bool faceRight = _player.transform.position.x > transform.position.x;
            if (_character.IsFacingRight != faceRight)
            {
                _character.Flip(faceRight);
            }
        }

        protected virtual void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Vector3 dir = Vector3.right * PlatformDetectionRange;
            Gizmos.DrawLine(transform.position, transform.position + dir);
            Gizmos.DrawLine(transform.position, transform.position - dir);
            Gizmos.DrawWireSphere(transform.position + Vector3.up * JumpRayLength, 0.1f);

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(_startPosition + Vector2.left * PatrolRange, _startPosition + Vector2.right * PatrolRange);
            Gizmos.DrawWireSphere(transform.position + Vector3.up * VerticalPlayerJumpThreshold, 0.1f);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, AttackRange);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, ViewDistance);

            Gizmos.color = Color.magenta;
            Vector3 diagDir = (Vector3.right + Vector3.up).normalized * PlatformDetectionRange;
            Gizmos.DrawLine(transform.position, transform.position + diagDir);
            Gizmos.DrawLine(transform.position, transform.position - diagDir);

            Gizmos.color = Color.yellow;
            Vector3 forward = _character?.IsFacingRight == true ? Vector3.right : Vector3.left;
            Gizmos.DrawLine(transform.position, transform.position + forward * WallDetectionDistance);
        }
    }
}
