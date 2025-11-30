using UnityEngine;
using UnityEngine.InputSystem;

namespace RedsUtils
{

    public class LocomotionBase : ScriptableObject
    {

        public InputActionReference inputActionReference;


        public virtual void ActionPerformed(InputAction.CallbackContext ctx)
        {
            
            

        }

    }
}