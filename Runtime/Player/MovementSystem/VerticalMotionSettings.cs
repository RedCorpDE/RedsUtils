using UnityEngine;

namespace RedsUtils.Player.MovementSystem.Modules
{
    [CreateAssetMenu(menuName = "RedsUtils/Player/Locomotion/Vertical Motion Settings", fileName = "VerticalMotionSettings")]
    public sealed class VerticalMotionSettings : ScriptableObject
    {
        [Tooltip("Use a negative value (e.g. -25).")]
        public float gravity = -25f;

        [Tooltip("Most negative allowed vertical velocity.")]
        public float terminalVelocity = -55f;

        [Tooltip("Small negative velocity to keep the controller grounded on slopes.")]
        public float groundedStickVelocity = -2f;
    }
}