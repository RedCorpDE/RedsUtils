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

            private Vector2Int _gridPos; // world-grid coordinate in XZ
            private bool _hasGridPos;

            public Runtime(PlayerContext ctx, LocomotionModuleGridBased def) : base(ctx) => _def = def;

            public override void Enable()
            {
                _moveAction = InputActionResolver.Resolve(Ctx.PlayerInput, _def.move);
                if (_moveAction == null) return;

                _moveAction.performed += OnMove;
                _moveAction.canceled += OnMove;

                InitGridPos();
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
                    InitGridPos();

                // If no input: do nothing
                if (_move.sqrMagnitude < _def.inputDeadzone * _def.inputDeadzone)
                    return;

                // 1) Convert input into a desired world direction (camera-relative OR character-relative)
                DirectionSpace.GetPlanarBasis(Ctx.Transform, Ctx.MainCamera, _def.cameraRelative, out var forward, out var right);
                Vector3 desired = forward * _move.y + right * _move.x;
                desired.y = 0f;

                if (desired.sqrMagnitude < 0.0001f)
                    return;

                desired.Normalize();

                // 2) Quantize desired direction to world-grid step (4-way or 8-way)
                Vector2Int step = _def.allowDiagonal ? Quantize8Way(desired) : Quantize4Way(desired);
                if (step == Vector2Int.zero)
                    return;

                // 3) Current target world position for current grid cell
                Vector3 pos = Ctx.Transform.position;
                Vector3 currentCellWorld = new Vector3(_gridPos.x * _def.cellSize, pos.y, _gridPos.y * _def.cellSize);

                // 4) If we are close enough to the current cell center, advance to next cell when input persists
                float toCell = Vector3.Distance(new Vector3(pos.x, currentCellWorld.y, pos.z), currentCellWorld);
                if (toCell < 0.02f)
                    _gridPos += step;

                // 5) Move toward the (possibly updated) target cell
                Vector3 target = new Vector3(_gridPos.x * _def.cellSize, pos.y, _gridPos.y * _def.cellSize);
                Vector3 toTarget = target - pos;
                toTarget.y = 0f;

                if (toTarget.sqrMagnitude < 0.0004f)
                    return;

                // Compute displacement clamped by speed
                float maxDist = _def.stepSpeed * dt;
                Vector3 displacement = Vector3.ClampMagnitude(toTarget, maxDist);

                // Convert displacement to velocity for motor
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

            private void InitGridPos()
            {
                var pos = Ctx.Transform.position;
                int gx = Mathf.RoundToInt(pos.x / _def.cellSize);
                int gz = Mathf.RoundToInt(pos.z / _def.cellSize);
                _gridPos = new Vector2Int(gx, gz);
                _hasGridPos = true;
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
                // Angle in XZ plane (0 = +Z)
                float angle = Mathf.Atan2(desired.x, desired.z) * Mathf.Rad2Deg;
                angle = (angle + 360f) % 360f;

                int octant = Mathf.RoundToInt(angle / 45f) % 8;

                return octant switch
                {
                    0 => new Vector2Int(0, 1),   // +Z
                    1 => new Vector2Int(1, 1),   // +X+Z
                    2 => new Vector2Int(1, 0),   // +X
                    3 => new Vector2Int(1, -1),  // +X-Z
                    4 => new Vector2Int(0, -1),  // -Z
                    5 => new Vector2Int(-1, -1), // -X-Z
                    6 => new Vector2Int(-1, 0),  // -X
                    7 => new Vector2Int(-1, 1),  // -X+Z
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
