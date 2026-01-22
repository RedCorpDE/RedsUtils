using UnityEngine.InputSystem;

namespace RedsUtils.Player.MovementSystem
{
    public static class InputActionResolver
    {
        public static InputAction Resolve(PlayerInput playerInput, InputActionReference reference)
        {
            if (playerInput == null || reference == null || reference.action == null)
                return null;

            // Prefer resolving from the player's instantiated actions by ID (most robust).
            var id = reference.action.id.ToString();
            var resolved = playerInput.actions.FindAction(id, throwIfNotFound: false);

            // Fallback to name if needed.
            return resolved ?? playerInput.actions.FindAction(reference.action.name, throwIfNotFound: false);
        }
    }
}