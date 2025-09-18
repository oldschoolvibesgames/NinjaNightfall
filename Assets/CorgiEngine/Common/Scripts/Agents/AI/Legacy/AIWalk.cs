using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{
    [RequireComponent(typeof(CharacterHorizontalMovement))]
    [AddComponentMenu("Corgi Engine/Character/AI/Legacy/AI Walk")]
    public class AIWalk : CorgiMonoBehaviour
    {
        public enum WalkBehaviours { Patrol, MoveOnSight, IdleUntilSeePlayer, ChaseAndAttack, AttackOnSight }

        public WalkBehaviours WalkBehaviour = WalkBehaviours.Patrol;

        [Header("Obstacle Detection")]
        public bool ChangeDirectionOnWall = true;
        public bool AvoidFalling = false;
        public Vector3 HoleDetectionOffset = Vector3.zero;
        public float HoleDetectionRaycastLength = 1f;

        [Header("Move on Sight")]
        public float ViewDistance = 10f;
        public float StopDistance = 1f;
        public Vector3 MoveOnSightRayOffset = Vector3.zero;
        public LayerMask MoveOnSightLayer = LayerManager.PlayerLayerMask;
        public LayerMask MoveOnSightObstaclesLayer = LayerManager.ObstaclesLayerMask;
        public Vector3 ObstacleDetectionRayOffset = Vector3.zero;
        public bool ResetPositionOnDeath = true;

        [Header("Chase and Attack")]
        public float AttackRange = 1.5f;
        public float AttackPauseDuration = 1f;

        [Header("Attack Settings")]
        public GameObject AttackHitbox;

        protected CorgiController _controller;
        protected Character _character;
        protected Health _health;
        protected CharacterHorizontalMovement _characterHorizontalMovement;
        protected Vector2 _direction;
        protected Vector2 _startPosition;
        protected Vector2 _initialDirection;
        protected Vector3 _initialScale;
        protected float _distanceToTarget;
        protected Vector2 _raycastOrigin;
        protected Vector2 _offset;

        protected bool _isPausing = false;
        protected GameObject _player;
        protected float _pauseTimer = 0f;
        protected Animator _animator;

        protected virtual void Start()
        {
            Initialization();
        }

        protected virtual void Initialization()
        {
            _controller = this.gameObject.GetComponentInParent<CorgiController>();
            _character = this.gameObject.GetComponentInParent<Character>();
            _characterHorizontalMovement = _character?.FindAbility<CharacterHorizontalMovement>();
            _health = this.gameObject.GetComponent<Health>();
            _startPosition = transform.position;
            _direction = _character.IsFacingRight ? Vector2.right : Vector2.left;
            _initialDirection = _direction;
            _initialScale = transform.localScale;
            _animator = GetComponentInChildren<Animator>();
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

            switch (WalkBehaviour)
            {
                case WalkBehaviours.Patrol:
                    CheckForWalls();
                    CheckForHoles();
                    _characterHorizontalMovement.SetHorizontalMove(_direction.x);
                    break;

                case WalkBehaviours.MoveOnSight:
                    CheckForTargetWithDistance();
                    _characterHorizontalMovement.SetHorizontalMove(_direction.x);
                    break;

                case WalkBehaviours.IdleUntilSeePlayer:
                    CheckIdleUntilSeePlayer();
                    _characterHorizontalMovement.SetHorizontalMove(_direction.x);
                    break;

                case WalkBehaviours.ChaseAndAttack:
                    CheckChaseAndAttack();
                    _characterHorizontalMovement.SetHorizontalMove(_direction.x);
                    break;

                case WalkBehaviours.AttackOnSight:
                    CheckAttackOnSight();
                    _characterHorizontalMovement.SetHorizontalMove(0f);
                    break;
            }
        }

        protected virtual void CheckForWalls()
        {
            if (!ChangeDirectionOnWall) return;
            if ((_direction.x < 0 && _controller.State.IsCollidingLeft) || (_direction.x > 0 && _controller.State.IsCollidingRight))
            {
                ChangeDirection();
            }
        }

        protected virtual void CheckForHoles()
        {
            if (!AvoidFalling || !_controller.State.IsGrounded) return;
            if (_character.IsFacingRight)
            {
                _raycastOrigin = transform.position + (_controller.Bounds.x / 2 + HoleDetectionOffset.x) * transform.right + HoleDetectionOffset.y * transform.up;
            }
            else
            {
                _raycastOrigin = transform.position - (_controller.Bounds.x / 2 + HoleDetectionOffset.x) * transform.right + HoleDetectionOffset.y * transform.up;
            }
            RaycastHit2D raycast = MMDebug.RayCast(_raycastOrigin, -transform.up, HoleDetectionRaycastLength,
                _controller.PlatformMask | _controller.MovingPlatformMask | _controller.OneWayPlatformMask | _controller.MovingOneWayPlatformMask,
                Color.gray, true);
            if (!raycast) ChangeDirection();
        }

        protected virtual void CheckForTargetWithDistance()
        {
            _direction = Vector2.zero;
            _distanceToTarget = Mathf.Infinity;

            if (_player == null) return;

            _distanceToTarget = Vector2.Distance(_player.transform.position, transform.position);
            if (_distanceToTarget <= ViewDistance)
            {
                _direction = (_player.transform.position.x > transform.position.x) ? Vector2.right : Vector2.left;
            }
            else
            {
                _direction = Vector2.zero;
            }
        }

        protected virtual void CheckIdleUntilSeePlayer()
        {
            if (_isPausing) { _direction = Vector2.zero; return; }
            CheckForTargetWithDistance();
            if (_distanceToTarget <= AttackRange)
            {
                TriggerAttack();
                StartCoroutine(PauseBeforeContinue(AttackPauseDuration));
                _direction = Vector2.zero;
            }
        }

        protected virtual void CheckChaseAndAttack()
        {
            if (_isPausing) { _direction = Vector2.zero; return; }
            CheckForTargetWithDistance();
            if (_distanceToTarget <= AttackRange)
            {
                TriggerAttack();
                StartCoroutine(PauseBeforeContinue(AttackPauseDuration));
                _direction = Vector2.zero;
            }
        }

        protected virtual void CheckAttackOnSight()
        {
            if (_isPausing) { _direction = Vector2.zero; return; }
            CheckForTargetWithDistance();
            if (_distanceToTarget <= AttackRange)
            {
                TriggerAttack();
                StartCoroutine(PauseBeforeContinue(AttackPauseDuration));
                _direction = Vector2.zero;
            }
            else
            {
                _direction = Vector2.zero;
            }
        }

        protected virtual void TriggerAttack()
        {
            if (_animator != null)
            {
                _animator.SetTrigger("Attack");
                _animator.SetBool("IsAttacking", true);
                StartCoroutine(ResetIsAttacking());
            }
        }

        protected virtual IEnumerator ResetIsAttacking()
        {
            yield return new WaitForSeconds(AttackPauseDuration);
            if (_animator != null)
            {
                _animator.SetBool("IsAttacking", false);
            }
        }

        protected virtual IEnumerator PauseBeforeContinue(float pauseTime)
        {
            _isPausing = true;
            yield return new WaitForSeconds(pauseTime);
            _isPausing = false;
        }

        protected virtual void ChangeDirection()
        {
            _direction = -_direction;
        }

        protected virtual void OnRevive()
        {
            _direction = _character.IsFacingRight ? Vector2.right : Vector2.left;
            transform.localScale = _initialScale;
            if (ResetPositionOnDeath) transform.position = _startPosition;
        }

        protected virtual void OnEnable()
        {
            if (_health != null) _health.OnRevive += OnRevive;
        }

        protected virtual void OnDisable()
        {
            if (_health != null) _health.OnRevive -= OnRevive;
        }

        protected virtual void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, AttackRange);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, ViewDistance);
        }
    }
}
