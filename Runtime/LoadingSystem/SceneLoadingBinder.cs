namespace RedsUtils.LoadingSystem
{
    using System;
    using Sirenix.OdinInspector;
    using UnityEngine;

    public class SceneLoadingBinder : MonoBehaviour
    {
    
        [SerializeField]
        bool fixedScene = false;
    
        [SerializeField, ShowIf("fixedScene")]
        string sceneName;
    
        public bool onStart = false;


        private void Start()
        {
        
            if(onStart)
                SwitchScene(sceneName);
        
        }

        public void SwitchScene(string scene)
        {
        
            SceneLoadingManager.SwitchScene(scene);
        
        }
    
    }
}