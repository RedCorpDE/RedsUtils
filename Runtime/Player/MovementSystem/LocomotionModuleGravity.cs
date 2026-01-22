using UnityEngine;

namespace RedsUtils.Player.MovementSystem.Modules
{
    [CreateAssetMenu(menuName = "RedsUtils/Player/Locomotion/Gravity", fileName = "LM_Gravity")]
    public sealed class LocomotionModuleGravity : LocomotionModuleBase
    {
        public VerticalMotionSettings settings;

        public override LocomotionRuntime CreateRuntime(PlayerContext context)
            => new Runtime(context, this);

        private sealed class Runtime : LocomotionRuntime
        {
            private readonly LocomotionModuleGravity _def;

            public Runtime(PlayerContext ctx, LocomotionModuleGravity def) : base(ctx) => _def = def;

            public override void Tick(float dt)
            {
                if (_def.settings == null) return;

                var motor = Ctx.Motor;

                // If grounded and falling, stick to ground
                if (motor.IsGrounded && motor.VerticalVelocity <= 0f)
                {
                    motor.SetVerticalVelocity(_def.settings.groundedStickVelocity);
                    return;
                }

                // Apply gravity
                motor.AddVerticalVelocity(_def.settings.gravity * dt);

                // Clamp terminal
                if (motor.VerticalVelocity < _def.settings.terminalVelocity)
                    motor.SetVerticalVelocity(_def.settings.terminalVelocity);
            }
        }
    }
}