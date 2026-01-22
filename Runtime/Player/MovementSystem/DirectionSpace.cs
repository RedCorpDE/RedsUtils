using UnityEngine;

namespace RedsUtils.Player.MovementSystem
{
    public static class DirectionSpace
    {
        public static void GetPlanarBasis(
            Transform character,
            Camera cam,
            bool cameraRelative,
            out Vector3 forward,
            out Vector3 right)
        {
            Transform t = (cameraRelative && cam != null) ? cam.transform : character;

            forward = Vector3.ProjectOnPlane(t.forward, Vector3.up).normalized;
            right   = Vector3.ProjectOnPlane(t.right, Vector3.up).normalized;

            // Fallback safety
            if (forward.sqrMagnitude < 0.0001f) forward = Vector3.forward;
            if (right.sqrMagnitude   < 0.0001f) right   = Vector3.right;
        }
    }
}