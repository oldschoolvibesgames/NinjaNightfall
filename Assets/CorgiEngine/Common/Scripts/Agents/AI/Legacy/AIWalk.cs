using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{
    [RequireComponent(typeof(CharacterHorizontalMovement))]
    [AddComponentMenu("Corgi Engine/Character/AI/Legacy/AI Walk")]
    public class AIWalk : CorgiMonoBehaviour
    {
        public enum WalkBehaviours { Patrol, MoveOnSight, IdleUntilSeePlayer }

        [Tooltip("The agent's walk behaviour")]
        public WalkBehaviours WalkBehaviour = WalkBehaviours.Patrol;

        [Header("Obstacle Detection")]
        public bool ChangeDirectionOnWall = true;
        public LayerMask WallLayers; // Nova variável para as camadas de parede
        public bool AvoidFalling = false;
        public Vector3 HoleDetectionOffset = new Vector3(0, 0, 0);
        public float HoleDetectionRaycastLength = 1f;

        [Header("Move on Sight")]
        public float ViewDistance = 10f;
        public float StopDistance = 1f;
        public Vector3 MoveOnSightRayOffset = new Vector3(0, 0, 0);
        public LayerMask MoveOnSightLayer = LayerManager.PlayerLayerMask;
        public LayerMask MoveOnSightObstaclesLayer = LayerManager.ObstaclesLayerMask;
        public Vector3 ObstacleDetectionRayOffset = new Vector3(0, 0, 0);

        public bool ResetPositionOnDeath = true;

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
        }

        protected virtual void Update()
        {
            if (_character == null ||
                _character.ConditionState.CurrentState == CharacterStates.CharacterConditions.Dead ||
                _character.ConditionState.CurrentState == CharacterStates.CharacterConditions.Frozen)
            {
                return;
            }

            switch (WalkBehaviour)
            {
                case WalkBehaviours.Patrol:
                    CheckForWalls();
                    CheckForHoles();
                    _characterHorizontalMovement.SetHorizontalMove(_direction.x);
                    break;

                case WalkBehaviours.MoveOnSight:
                    CheckForTarget();
                    _characterHorizontalMovement.SetHorizontalMove(_direction.x);
                    break;

                case WalkBehaviours.IdleUntilSeePlayer:
                    CheckIdleUntilSeePlayer();
                    _characterHorizontalMovement.SetHorizontalMove(_direction.x);
                    break;
            }
        }

        protected virtual void CheckForWalls()
        {
            if (!ChangeDirectionOnWall) return;

            // Using a raycast to detect walls based on WallLayers
            Vector2 wallRaycastOrigin;
            if (_character.IsFacingRight)
            {
                wallRaycastOrigin = transform.position + (_controller.Bounds.x / 2 + 0.1f) * transform.right; // Offset slightly in direction of movement
            }
            else
            {
                wallRaycastOrigin = transform.position - (_controller.Bounds.x / 2 + 0.1f) * transform.right; // Offset slightly in direction of movement
            }

            RaycastHit2D wallRaycast = MMDebug.RayCast(wallRaycastOrigin, _direction, 0.1f, WallLayers, Color.yellow, true);

            if ((_direction.x < 0 && _controller.State.IsCollidingLeft) || (_direction.x > 0 && _controller.State.IsCollidingRight) || wallRaycast)
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

            if (!raycast)
            {
                ChangeDirection();
            }
        }

        protected virtual void CheckForTarget()
        {
            if (WalkBehaviour != WalkBehaviours.MoveOnSight) return;

            bool hit = false;
            _distanceToTarget = 0;
            _offset = MoveOnSightRayOffset;

            if (!_character.IsFacingRight) _offset.x = -_offset.x;
            _raycastOrigin = (Vector2)transform.position + _offset;

            RaycastHit2D raycast = MMDebug.RayCast(_raycastOrigin, Vector2.left, ViewDistance, MoveOnSightLayer, Color.green, true);
            if (raycast)
            {
                hit = true;
                _direction = Vector2.left;
                _distanceToTarget = raycast.distance;
            }

            raycast = MMDebug.RayCast(_raycastOrigin, Vector2.right, ViewDistance, MoveOnSightLayer, Color.green, true);
            if (raycast)
            {
                hit = true;
                _direction = Vector2.right;
                _distanceToTarget = raycast.distance;
            }

            if (!hit || _distanceToTarget <= StopDistance)
            {
                _direction = Vector2.zero;
            }
            else
            {
                _offset = ObstacleDetectionRayOffset;
                if (!_character.IsFacingRight) _offset.x = -_offset.x;
                _raycastOrigin = (Vector2)transform.position + _offset;

                RaycastHit2D raycastObstacle = MMDebug.RayCast(_raycastOrigin, _direction, ViewDistance, MoveOnSightObstaclesLayer, Color.gray, true);
                if (raycastObstacle && _distanceToTarget > raycastObstacle.distance)
                {
                    _direction = Vector2.zero;
                }
            }
        }

        protected virtual void CheckIdleUntilSeePlayer()
        {
            if (_isPausing)
            {
                _direction = Vector2.zero;
                return;
            }

            bool hit = false;
            _direction = Vector2.zero;
            _distanceToTarget = 0;
            _offset = MoveOnSightRayOffset;

            if (!_character.IsFacingRight) _offset.x = -_offset.x;
            _raycastOrigin = (Vector2)transform.position + _offset;

            RaycastHit2D raycast = MMDebug.RayCast(_raycastOrigin, Vector2.left, ViewDistance, MoveOnSightLayer, Color.cyan, true);
            if (raycast)
            {
                hit = true;
                _direction = Vector2.left;
                _distanceToTarget = raycast.distance;
            }

            raycast = MMDebug.RayCast(_raycastOrigin, Vector2.right, ViewDistance, MoveOnSightLayer, Color.cyan, true);
            if (raycast)
            {
                hit = true;
                _direction = Vector2.right;
                _distanceToTarget = raycast.distance;
            }

            if (hit)
            {
                _offset = ObstacleDetectionRayOffset;
                if (!_character.IsFacingRight) _offset.x = -_offset.x;
                _raycastOrigin = (Vector2)transform.position + _offset;

                RaycastHit2D raycastObstacle = MMDebug.RayCast(_raycastOrigin, _direction, ViewDistance, MoveOnSightObstaclesLayer, Color.gray, true);
                if (raycastObstacle && _distanceToTarget > raycastObstacle.distance)
                {
                    _direction = Vector2.zero;
                    return;
                }

                if (_distanceToTarget <= 1f)
                {
                    StartCoroutine(PauseBeforeContinue());
                    _direction = Vector2.zero;
                }
            }
        }

        protected virtual IEnumerator PauseBeforeContinue()
        {
            _isPausing = true;
            yield return new WaitForSeconds(0.3f);
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
            if (ResetPositionOnDeath)
            {
                transform.position = _startPosition;
            }
        }

        protected virtual void OnEnable()
        {
            if (_health != null)
            {
                _health.OnRevive += OnRevive;
            }
        }

        protected virtual void OnDisable()
        {
            if (_health != null)
            {
                _health.OnRevive -= OnRevive;
            }
        }
    }
}