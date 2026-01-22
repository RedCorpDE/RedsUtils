using UnityEngine;
using UnityEngine.InputSystem;

namespace RedsUtils.Player.MovementSystem.Modules
{
    [CreateAssetMenu(menuName = "RedsUtils/Player/Locomotion/Grid Walk", fileName = "LM_GridWalk")]
    public sealed class LocomotionModuleGridBased : LocomotionModuleBase
    {
        [Header("Input")]
        public InputActionReference move; // Vector2

        [Header("Space")]
        [Tooltip("If true: interpret input relative to camera. If false: relative to character.")]
        public bool cameraRelative = true;

        [Header("Grid")]
        public float cellSize = 1f;
        public float stepSpeed = 4f;
        public float inputDeadzone = 0.25f;

        [Tooltip("If false: 4-way. If true: 8-way.")]
        public bool allowDiagonal = false;

        [Header("Facing")]
        public bool rotateToStepDirection = true;
        public float rotationSharpness = 18f;

        public override LocomotionRuntime CreateRuntime(PlayerContext context)
            => new Runtime(context, this);

        private sealed class Runtime : LocomotionRuntime
        {
            private readonly LocomotionModuleGridBased _def;
            private InputAction _moveAction;
            private Vector2 _move;

            private Vector2Int _gridPos;      // current cell (XZ)
            private bool _hasGridPos;

            private bool _isStepping;
            private Vector2Int _stepDir;      // current step direction (grid)
            private Vector2Int _nextGridPos;  // target cell for current step
            private Vector3 _targetWorld;     // world-space center of _nextGridPos (y ignored for planar)

            private Vector2Int _queuedDir;    // optional: next direction to take after current step

            // Optional: stall detection (prevents infinite pushing against walls)
            private float _stuckTime;
            private float _lastDistToTarget;

            public Runtime(PlayerContext ctx, LocomotionModuleGridBased def) : base(ctx) => _def = def;

            public override void Enable()
            {
                _moveAction = InputActionResolver.Resolve(Ctx.PlayerInput, _def.move);
                if (_moveAction == null) return;

                _moveAction.performed += OnMove;
                _moveAction.canceled += OnMove;

                InitGridPosFromCurrentPosition();
                _isStepping = false;
                _queuedDir = Vector2Int.zero;
            }

            public override void Disable()
            {
                if (_moveAction == null) return;

                _moveAction.performed -= OnMove;
                _moveAction.canceled -= OnMove;
                _moveAction = null;
            }

            public override void Tick(float dt)
            {
                if (!_hasGridPos)
                    InitGridPosFromCurrentPosition();

                // Compute desired direction from current input (can be zero).
                Vector2Int desiredDir = ReadDesiredGridDirectionFromInput();

                if (_isStepping)
                {
                    // Allow direction changes to be queued while stepping (optional but usually desired).
                    if (desiredDir != Vector2Int.zero && desiredDir != _stepDir)
                        _queuedDir = desiredDir;

                    // If we already arrived (or got pushed there) complete the step now.
                    float dist = PlanarDistanceToTarget();
                    if (dist <= 0.01f)
                    {
                        CompleteStep();

                        // After completing, if a direction is queued, start it next.
                        if (_queuedDir != Vector2Int.zero)
                        {
                            StartStep(_queuedDir);
                            _queuedDir = Vector2Int.zero;
                        }
                        else if (desiredDir != Vector2Int.zero)
                        {
                            StartStep(desiredDir);
                        }

                        // If we started a new step, we will fall through and move toward its target this frame.
                        // If not, we're idle.
                        if (!_isStepping)
                            return;
                    }

                    // Move toward current step target, regardless of whether input is held.
                    MoveTowardsTarget(dt);
                    return;
                }

                // Not currently stepping: only start stepping if input is meaningful.
                if (desiredDir == Vector2Int.zero)
                    return;

                StartStep(desiredDir);
                MoveTowardsTarget(dt);
            }

            private void StartStep(Vector2Int dir)
            {
                // Rebase to the nearest grid cell at the moment we start a step.
                // This keeps the system robust if something pushed us off-center.
                _gridPos = WorldToGrid(Ctx.Transform.position);

                _stepDir = dir;
                _nextGridPos = _gridPos + _stepDir;

                var pos = Ctx.Transform.position;
                _targetWorld = new Vector3(_nextGridPos.x * _def.cellSize, pos.y, _nextGridPos.y * _def.cellSize);

                _isStepping = true;

                _stuckTime = 0f;
                _lastDistToTarget = PlanarDistanceToTarget();
            }

            private void CompleteStep()
            {
                _gridPos = _nextGridPos;
                _isStepping = false;
                _stepDir = Vector2Int.zero;
                _stuckTime = 0f;
                _lastDistToTarget = 0f;
            }

            private void MoveTowardsTarget(float dt)
            {
                Vector3 pos = Ctx.Transform.position;

                Vector3 toTarget = _targetWorld - pos;
                toTarget.y = 0f;

                float dist = toTarget.magnitude;
                if (dist <= 0.0001f)
                    return;

                // Stall detection: if distance is not decreasing, we may be blocked by a wall.
                if (dist >= _lastDistToTarget - 0.0005f)
                    _stuckTime += dt;
                else
                    _stuckTime = 0f;

                _lastDistToTarget = dist;

                // If blocked for a short while, cancel the step and rebase grid position.
                if (_stuckTime > 0.20f)
                {
                    _isStepping = false;
                    _queuedDir = Vector2Int.zero;
                    InitGridPosFromCurrentPosition();
                    return;
                }

                float maxDist = _def.stepSpeed * dt;
                Vector3 displacement = Vector3.ClampMagnitude(toTarget, maxDist);

                // Drive motor with velocity equivalent to the displacement we want this frame.
                Ctx.Motor.SetPlanarVelocity(displacement / Mathf.Max(dt, 0.0001f));

                if (_def.rotateToStepDirection && displacement.sqrMagnitude > 0.0001f)
                {
                    var dir = displacement.normalized;
                    var targetRot = Quaternion.LookRotation(dir, Vector3.up);
                    Ctx.Transform.rotation = Quaternion.Slerp(
                        Ctx.Transform.rotation,
                        targetRot,
                        1f - Mathf.Exp(-_def.rotationSharpness * dt));
                }
            }

            private Vector2Int ReadDesiredGridDirectionFromInput()
            {
                if (_move.sqrMagnitude < _def.inputDeadzone * _def.inputDeadzone)
                    return Vector2Int.zero;

                DirectionSpace.GetPlanarBasis(Ctx.Transform, Ctx.MainCamera, _def.cameraRelative, out var forward, out var right);

                Vector3 desired = forward * _move.y + right * _move.x;
                desired.y = 0f;

                if (desired.sqrMagnitude < 0.0001f)
                    return Vector2Int.zero;

                desired.Normalize();
                return _def.allowDiagonal ? Quantize8Way(desired) : Quantize4Way(desired);
            }

            private float PlanarDistanceToTarget()
            {
                var pos = Ctx.Transform.position;
                float dx = _targetWorld.x - pos.x;
                float dz = _targetWorld.z - pos.z;
                return Mathf.Sqrt(dx * dx + dz * dz);
            }

            private void InitGridPosFromCurrentPosition()
            {
                _gridPos = WorldToGrid(Ctx.Transform.position);
                _hasGridPos = true;
            }

            private Vector2Int WorldToGrid(Vector3 worldPos)
            {
                int gx = Mathf.RoundToInt(worldPos.x / _def.cellSize);
                int gz = Mathf.RoundToInt(worldPos.z / _def.cellSize);
                return new Vector2Int(gx, gz);
            }

            private static Vector2Int Quantize4Way(Vector3 desired)
            {
                float dotF = Vector3.Dot(desired, Vector3.forward);
                float dotR = Vector3.Dot(desired, Vector3.right);

                if (Mathf.Abs(dotF) >= Mathf.Abs(dotR))
                    return new Vector2Int(0, dotF >= 0f ? 1 : -1);

                return new Vector2Int(dotR >= 0f ? 1 : -1, 0);
            }

            private static Vector2Int Quantize8Way(Vector3 desired)
            {
                float angle = Mathf.Atan2(desired.x, desired.z) * Mathf.Rad2Deg;
                angle = (angle + 360f) % 360f;

                int octant = Mathf.RoundToInt(angle / 45f) % 8;

                return octant switch
                {
                    0 => new Vector2Int(0, 1),
                    1 => new Vector2Int(1, 1),
                    2 => new Vector2Int(1, 0),
                    3 => new Vector2Int(1, -1),
                    4 => new Vector2Int(0, -1),
                    5 => new Vector2Int(-1, -1),
                    6 => new Vector2Int(-1, 0),
                    7 => new Vector2Int(-1, 1),
                    _ => Vector2Int.zero
                };
            }

            private void OnMove(InputAction.CallbackContext ctx)
            {
                _move = ctx.ReadValue<Vector2>();
            }
        }

    }
}
