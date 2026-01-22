using UnityEngine;
using UnityEngine.InputSystem;

namespace RedsUtils.Player.MovementSystem.Modules
{
    [CreateAssetMenu(menuName = "RedsUtils/Player/Locomotion/Move", fileName = "LM_Move")]
    public sealed class LocomotionModuleMovement : LocomotionModuleBase
    {
        [Header("Input")]
        public InputActionReference move; // Vector2

        [Header("Space")]
        [Tooltip("If true: forward/right from camera. If false: from character.")]
        public bool cameraRelative = true;

        [Header("Tuning")]
        public float speed = 5.5f;
        public float rotationSharpness = 18f;

        public override LocomotionRuntime CreateRuntime(PlayerContext context)
            => new Runtime(context, this);

        private sealed class Runtime : LocomotionRuntime
        {
            private readonly LocomotionModuleMovement _def;
            private InputAction _moveAction;
            private Vector2 _move;

            public Runtime(PlayerContext ctx, LocomotionModuleMovement def) : base(ctx) => _def = def;

            public override void Enable()
            {
                _moveAction = InputActionResolver.Resolve(Ctx.PlayerInput, _def.move);
                if (_moveAction == null) return;

                _moveAction.performed += OnMove;
                _moveAction.canceled += OnMove;
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
                DirectionSpace.GetPlanarBasis(Ctx.Transform, Ctx.MainCamera, _def.cameraRelative, out var forward, out var right);

                Vector3 desired = forward * _move.y + right * _move.x;
                if (desired.sqrMagnitude > 1f) desired.Normalize();

                Ctx.Motor.SetPlanarVelocity(desired * _def.speed);

                if (desired.sqrMagnitude > 0.0001f)
                {
                    var targetRot = Quaternion.LookRotation(desired, Vector3.up);
                    Ctx.Transform.rotation = Quaternion.Slerp(
                        Ctx.Transform.rotation,
                        targetRot,
                        1f - Mathf.Exp(-_def.rotationSharpness * dt));
                }
            }

            private void OnMove(InputAction.CallbackContext ctx)
            {
                _move = ctx.ReadValue<Vector2>();
            }
        }
    }
}
