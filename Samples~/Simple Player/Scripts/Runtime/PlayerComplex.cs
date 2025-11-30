using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RedsUtils
{
    [RequireComponent(typeof(CharacterController))]
    [AddComponentMenu("Utilities/Player/Simple Player")]
    public class PlayerComplex : PlayerSimple
    {

        [AssetList]
        public List<LocomotionBase> availableMovements = new List<LocomotionBase>();


        private void OnEnable()
        {

            for (int i = 0; i < availableMovements.Count; i++)
            {

                availableMovements[i].inputActionReference.action.performed += availableMovements[i].ActionPerformed;

            }
            
        }

        private void OnDisable()
        {
            
            for (int i = 0; i < availableMovements.Count; i++)
            {

                availableMovements[i].inputActionReference.action.performed -= availableMovements[i].ActionPerformed;

            }
            
        }
        
        
        
    }
}