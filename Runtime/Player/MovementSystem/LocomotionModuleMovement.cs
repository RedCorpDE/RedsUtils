using UnityEngine;
using UnityEngine.InputSystem;

namespace RedsUtils.Player.MovementSystem
{
    [CreateAssetMenu(menuName = "Utilities/Player/Locomotion - Movement")]
    public sealed class LocomotionModuleMovement : LocomotionModuleBase
    {

        [Header("Input")] public InputActionReference move; // expected Vector2

        [Header("Tuning")] public float speed = 5.5f;
        public float rotationSharpness = 18f;

        public override LocomotionRuntime CreateRuntime(PlayerContext context)
            => new Runtime(context, this);

        private sealed class Runtime : LocomotionRuntime
        {
            private readonly LocomotionModuleMovement _def;
            private InputAction _moveAction;

            private Vector2 _move;
            private Vector3 _planarVelocity;

            public Runtime(PlayerContext ctx, LocomotionModuleMovement def) : base(ctx) => _def = def;

            public override void Enable()
            {
                _moveAction = InputActionResolver.Resolve(Ctx.PlayerInput, _def.move);
                if (_moveAction == null) return;

                _moveAction.performed += OnMove;
                _moveAction.canceled += OnMove;
                // Usually you do NOT call Enable() here if PlayerInput handles it via action maps.
                // Only do it if you are not using PlayerInput to enable maps.
                // _moveAction.Enable();
            }

            public override void Disable()
            {
                if (_moveAction == null) return;

                _moveAction.performed -= OnMove;
                _moveAction.canceled -= OnMove;
                // _moveAction.Disable();
                _moveAction = null;
            }

            public override void Tick(float dt)
            {
                // Convert input to camera-relative move
                var cam = Ctx.MainCamera != null ? Ctx.MainCamera.transform : null;
                Vector3 forward =
                    cam ? Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized : Ctx.Transform.forward;
                Vector3 right = cam ? Vector3.ProjectOnPlane(cam.right, Vector3.up).normalized : Ctx.Transform.right;

                Vector3 desired = (forward * _move.y + right * _move.x);
                if (desired.sqrMagnitude > 1f) desired.Normalize();

                _planarVelocity = desired * _def.speed;

                // CharacterController.Move is typically done in Update (dt), unless you’re doing physics-based locomotion.
                Ctx.CharacterController.Move(_planarVelocity * dt);

                // Face move direction (optional)
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