using UnityEngine;

namespace RedsUtils.Player.MovementSystem
{
    public abstract class LocomotionModuleBase : ScriptableObject
    {
        /// <summary>
        /// Create a per-player runtime instance that can hold state and subscribe to input.
        /// </summary>
        public abstract LocomotionRuntime CreateRuntime(PlayerContext context);
    }

    public abstract class LocomotionRuntime
    {
        protected readonly PlayerContext Ctx;

        protected LocomotionRuntime(PlayerContext ctx) => Ctx = ctx;

        public virtual void Enable() { }
        public virtual void Disable() { }

        public virtual void Tick(float dt) { }
        public virtual void FixedTick(float fixedDt) { }
    }
}