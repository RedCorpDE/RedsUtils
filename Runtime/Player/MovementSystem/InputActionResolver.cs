using UnityEngine.InputSystem;

namespace RedsUtils.Player.MovementSystem
{
    public static class InputActionResolver
    {
        public static InputAction Resolve(PlayerInput playerInput, InputActionReference reference)
        {
            if (playerInput == null || reference == null || reference.action == null)
                return null;

            var id = reference.action.id.ToString();
            var resolved = playerInput.actions.FindAction(id, throwIfNotFound: false);
            return resolved ?? playerInput.actions.FindAction(reference.action.name, throwIfNotFound: false);
        }
    }
}