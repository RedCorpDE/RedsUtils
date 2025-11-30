using UnityEngine;
using UnityEngine.InputSystem;

namespace RedsUtils
{
    [CreateAssetMenu(menuName = "Utilities/Player/Locomotion")]
    public class LocomotionWalking : LocomotionBase
    {

        public override void ActionPerformed(InputAction.CallbackContext ctx)
        {
            
            Vector2 movement = ctx.ReadValue<Vector2>();
            
            

        }
    
    }
}