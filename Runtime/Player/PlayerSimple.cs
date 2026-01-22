using Unity.Cinemachine;
using UnityEngine;

namespace RedsUtils.Player
{
    [AddComponentMenu("Utilities/Player/Simple Player")]
    public class PlayerSimple : SingletonPersistent<PlayerSimple>
    {

        public Camera mainCamera;
        public CinemachineBrain cinemachineBrain;


        public virtual void ChangeBlendTime(float blendTime)
        {

            cinemachineBrain.DefaultBlend = new CinemachineBlendDefinition()
            {
                Time = blendTime
            };

        }

        public virtual void ResetBlendTime()
        {
            
            cinemachineBrain.DefaultBlend = new CinemachineBlendDefinition()
            {
                Time = 2f
            };
            
        }

    }
}