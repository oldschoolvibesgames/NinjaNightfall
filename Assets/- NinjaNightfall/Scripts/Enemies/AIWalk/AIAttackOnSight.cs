using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using UnityEngine.Events;

namespace MoreMountains.CorgiEngine
{
    [RequireComponent(typeof(CharacterHorizontalMovement))]
    [AddComponentMenu("Corgi Engine/Character/AI/Legacy/AI Attack On Sight")]
    public class AIAttackOnSight : CorgiMonoBehaviour
    {
        public float AttackRange = 1.5f;
        public float AttackPauseDuration = 1f;
        public GameObject AttackHitbox;
        public float ViewDistance = 10f;

        public UnityEvent onAttack;
        public UnityEvent onDead;

        protected CorgiController _controller;
        protected Character _character;
        protected Health _health;
        protected CharacterHorizontalMovement _characterHorizontalMovement;
        protected Vector2 _startPosition;
        protected Vector3 _initialScale;
        protected GameObject _player;
        protected Animator _animator;

        protected bool _isAttacking = false;
        protected float _distanceToTarget;
        protected bool _playerInView = false;
        protected bool _playerInAttackRange = false;

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
            _startPosition = transform.position;
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

            CheckAttackOnSight();
        }

        protected virtual void CheckAttackOnSight()
        {
            _distanceToTarget = Vector2.Distance(_player.transform.position, transform.position);
            _playerInView = _distanceToTarget <= ViewDistance;
            _playerInAttackRange = _distanceToTarget <= AttackRange;

            if (_playerInView)
            {
                FacePlayer();
            }

            if (_isAttacking)
            {
                return;
            }

            if (_playerInView && _playerInAttackRange)
            {
                StartCoroutine(AttackSequence());
            }
            else if (_animator != null)
            {
                _animator.SetBool("IsAttacking", false);
            }
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
                onAttack?.Invoke();
            }

            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            while (!stateInfo.IsName("Attack"))
            {
                yield return null;
                stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            }

            while (stateInfo.IsName("Attack") && stateInfo.normalizedTime < 1f)
            {
                yield return null;
                stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            }

            if (_animator != null)
            {
                _animator.SetBool("IsAttacking", false);
            }

            yield return new WaitForSeconds(AttackPauseDuration);
            _isAttacking = false;
        }

        protected virtual void OnRevive()
        {
            transform.localScale = _initialScale;
            if (_character != null)
            {
                if (_character.IsFacingRight)
                {
                    _character.Flip(true);
                }
                else
                {
                    _character.Flip(false);
                }
            }
            if (_health != null && _startPosition != null)
            {
                transform.position = _startPosition;
            }
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
