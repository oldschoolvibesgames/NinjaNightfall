using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{
    [RequireComponent(typeof(CharacterHorizontalMovement))]
    [AddComponentMenu("Corgi Engine/Character/AI/Legacy/AI Chase and Attack")]
    public class AIChaseAndAttack : CorgiMonoBehaviour
    {
        public bool RespectPlatformEdges = true;
        public float EdgeDetectionDistance = 0.5f;
        public LayerMask GroundLayer;
    
        public float ViewDistance = 10f;
        public float AttackRange = 1.5f;
        public float AttackPauseDuration = 1f;
        public GameObject AttackHitbox;
        public float PatrolSpeed = 1f;
        public float ChaseSpeed = 3f;
        public float StopDistance = 0.5f;
        public LayerMask ObstacleLayer;

        protected CorgiController _controller;
        protected Character _character;
        protected Health _health;
        protected CharacterHorizontalMovement _characterHorizontalMovement;
        protected GameObject _player;
        protected Animator _animator;
        protected Vector3 _initialScale;
        protected Vector3 _startPosition;

        protected Vector2 _direction;
        protected bool _isAttacking = false;
        protected bool _playerInView = false;
        protected bool _playerInRange = false;
        public float viewHeight;

        protected virtual void Start()
        {
            Initialization();
        }

        protected virtual void Initialization()
        {
            _controller = GetComponentInParent<CorgiController>();
            _character = GetComponentInParent<Character>();
            _characterHorizontalMovement = _character?.FindAbility<CharacterHorizontalMovement>();
            _health = GetComponent<Health>();
            _animator = GetComponentInChildren<Animator>();
            _initialScale = transform.localScale;
            _startPosition = transform.position;
            _direction = Vector2.left;
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

            _playerInRange = IsPlayerVisible(AttackRange);
            _playerInView = IsPlayerVisible(ViewDistance);

            if (_isAttacking) return;

            if (_playerInView)
            {
                FacePlayer();

                if (_playerInRange)
                {
                    StartCoroutine(AttackSequence());
                    _characterHorizontalMovement.SetHorizontalMove(0f);
                }
                else
                {
                    _characterHorizontalMovement.MovementSpeed = ChaseSpeed;
                    Vector2 moveDir = (_player.transform.position.x > transform.position.x) ? Vector2.right : Vector2.left;
                    _characterHorizontalMovement.SetHorizontalMove(moveDir.x);
                }
            }
            else
            {
                _characterHorizontalMovement.MovementSpeed = PatrolSpeed;
                Patrol();
            }
        }

        protected virtual void Patrol()
        {
            _characterHorizontalMovement.SetHorizontalMove(_direction.x);

            bool shouldFlip = false;
            
            if ((_direction.x < 0 && _controller.State.IsCollidingLeft) ||
                (_direction.x > 0 && _controller.State.IsCollidingRight))
            {
                shouldFlip = true;
            }
            
            if (RespectPlatformEdges && !IsGroundAhead())
            {
                shouldFlip = true;
            }

            if (shouldFlip)
            {
                _direction = -_direction;
            }
        }
        
        protected virtual bool IsGroundAhead()
        {
            Vector2 origin = transform.position + new Vector3(_direction.x * 0.5f, 0f);
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, EdgeDetectionDistance, GroundLayer);
            return hit.collider != null;
        }

        protected virtual void FacePlayer()
        {
            if (_player == null || _character == null) return;

            bool playerRight = _player.transform.position.x > transform.position.x;
            if (playerRight != _character.IsFacingRight)
            {
                _character.Flip(playerRight);

                if (AttackHitbox != null)
                {
                    Vector3 pos = AttackHitbox.transform.localPosition;
                    pos.x = -pos.x;
                    AttackHitbox.transform.localPosition = pos;
                }
            }
        }

        protected virtual IEnumerator AttackSequence()
        {
            _isAttacking = true;

            if (_animator != null)
            {
                _animator.SetTrigger("Attack");
                _animator.SetBool("IsAttacking", true);
            }

            yield return null;

            while (_animator.GetCurrentAnimatorStateInfo(0).IsName("Attack") &&
                   _animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            {
                yield return null;
            }

            if (_animator != null)
            {
                _animator.SetBool("IsAttacking", false);
            }

            yield return new WaitForSeconds(AttackPauseDuration);

            _isAttacking = false;
        }

        protected virtual bool IsPlayerVisible(float range)
        {
            if (_player == null) return false;

            Vector2 origin = transform.position;
            origin.y += viewHeight;
            Vector2 direction = (_character.IsFacingRight) ? Vector2.right : Vector2.left;
            RaycastHit2D hit = Physics2D.Raycast(origin, direction, range, ObstacleLayer);

            return hit.collider != null && hit.collider.gameObject.CompareTag("Player");
        }

        protected virtual void OnRevive()
        {
            transform.localScale = _initialScale;
            transform.position = _startPosition;
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
            Character character = GetComponentInParent<Character>();
            if (character != null)
            {
                Vector3 dir = character.IsFacingRight ? Vector3.right : Vector3.left;
                var startView = transform.position;
                startView.y += viewHeight;
                
                Gizmos.color = Color.green;
                Gizmos.DrawLine(startView, startView + dir * ViewDistance);

                Gizmos.color = Color.red;
                Gizmos.DrawLine(startView, startView + dir * AttackRange);
            }
            
            if (RespectPlatformEdges)
            {
                Vector2 origin = Application.isPlaying ? 
                    (Vector2)(transform.position + new Vector3(_direction.x * 0.5f, 0f)) :
                    (Vector2)(transform.position + Vector3.right * 0.5f); // Fallback para editor

                Gizmos.color = new Color(1f, 0.5f, 0f, 1f);
                Gizmos.DrawLine(origin, origin + Vector2.down * EdgeDetectionDistance);
            }
        }

    }
}
