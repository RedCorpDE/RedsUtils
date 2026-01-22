using UnityEngine;
using UnityEngine.InputSystem;

namespace RedsUtils.Player.MovementSystem.Modules
{
    [CreateAssetMenu(menuName = "RedsUtils/Player/Locomotion/Jump", fileName = "LM_Jump")]
    public sealed class LocomotionModuleVertical : LocomotionModuleBase
    {
        [Header("Input")]
        public InputActionReference jump; // Button

        [Header("Vertical Motion")]
        public VerticalMotionSettings settings;

        [Header("Tuning")]
        public float jumpHeight = 1.3f;

        [Tooltip("Allow jump shortly after leaving ground.")]
        public float coyoteTime = 0.10f;

        [Tooltip("Allow jump shortly before landing.")]
        public float jumpBufferTime = 0.10f;

        public override LocomotionRuntime CreateRuntime(PlayerContext context)
            => new Runtime(context, this);

        private sealed class Runtime : LocomotionRuntime
        {
            private readonly LocomotionModuleVertical _def;
            private InputAction _jumpAction;

            private float _lastGroundedTime;
            private float _jumpPressedTime;
            private bool _jumpQueued;

            public Runtime(PlayerContext ctx, LocomotionModuleVertical def) : base(ctx) => _def = def;

            public override void Enable()
            {
                _jumpAction = InputActionResolver.Resolve(Ctx.PlayerInput, _def.jump);
                if (_jumpAction == null) return;

                _jumpAction.performed += OnJump;
            }

            public override void Disable()
            {
                if (_jumpAction != null)
                    _jumpAction.performed -= OnJump;

                _jumpAction = null;
            }

            public override void Tick(float dt)
            {
                if (_def.settings == null) return;

                var motor = Ctx.Motor;

                if (motor.IsGrounded)
                    _lastGroundedTime = Time.time;

                // Expire buffered input
                if (_jumpQueued && (Time.time - _jumpPressedTime) > _def.jumpBufferTime)
                    _jumpQueued = false;

                // Can we jump?
                bool canUseCoyote = (Time.time - _lastGroundedTime) <= _def.coyoteTime;
                if (!_jumpQueued || !canUseCoyote) return;

                // Execute jump
                float g = _def.settings.gravity; // negative expected
                float jumpVelocity = Mathf.Sqrt(2f * _def.jumpHeight * -g);

                motor.SetVerticalVelocity(jumpVelocity);
                _jumpQueued = false;
            }

            private void OnJump(InputAction.CallbackContext ctx)
            {
                _jumpPressedTime = Time.time;
                _jumpQueued = true;
            }
        }
    }
}
