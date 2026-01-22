using UnityEngine;

namespace RedsUtils.Player.MovementSystem
{
    public sealed class LocomotionMotor
    {
        private readonly CharacterController _cc;

        public Vector3 PlanarVelocity { get; private set; }
        public float VerticalVelocity { get; private set; }

        public bool WasGrounded { get; private set; }
        public bool IsGrounded { get; private set; }

        public CollisionFlags LastCollisionFlags { get; private set; }

        public LocomotionMotor(CharacterController cc)
        {
            _cc = cc;
            IsGrounded = cc.isGrounded;
        }

        /// <summary>Call once per Update before modules Tick.</summary>
        public void BeginFrame()
        {
            WasGrounded = IsGrounded;
            PlanarVelocity = Vector3.zero;
        }

        public void SetPlanarVelocity(Vector3 v) => PlanarVelocity = v;
        public void AddPlanarVelocity(Vector3 v) => PlanarVelocity += v;

        public void SetVerticalVelocity(float v) => VerticalVelocity = v;
        public void AddVerticalVelocity(float dv) => VerticalVelocity += dv;

        /// <summary>Call once per Update after modules Tick.</summary>
        public void Apply(float dt)
        {
            Vector3 delta = (PlanarVelocity + Vector3.up * VerticalVelocity) * dt;

            LastCollisionFlags = _cc.Move(delta);
            IsGrounded = (LastCollisionFlags & CollisionFlags.Below) != 0;

            // Stop upward velocity when hitting ceiling
            if ((LastCollisionFlags & CollisionFlags.Above) != 0 && VerticalVelocity > 0f)
                VerticalVelocity = 0f;
        }
    }
}