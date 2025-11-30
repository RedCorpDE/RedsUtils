using System;
using UnityEngine;


namespace RedsUtils
{


    public class GameManager : SingletonPersistent<GameManager>
    {

        protected RuntimeSettingsPropertyBase UseQuitSystemProperty;
    

        public static event Action OnTryingToQuit;
        public static event Action OnQuitConfirm;


        public override void Awake()
        {
            base.Awake();

#if UNITY_ANDROID
        Unity.XR.Oculus.Performance.TrySetDisplayRefreshRate(90);
#endif

        }

        private void Start()
        {

            UseQuitSystemProperty = RuntimeSettings.instance.GetProperty("UseQuitSystem");

            if (UseQuitSystemProperty == null)
            {

                UseQuitSystemProperty = new RuntimeSettingsPropertyBase()
                {
                    name = "UseQuitSystem",
                    Value = Boolean.FalseString,
                };
                
                RuntimeSettings.instance.AddProperty(UseQuitSystemProperty);
                
            }
            else
            {
                
                UseQuitSystemProperty.Value = RuntimeSettings.instance.GetPropertyValue("UseQuitSystem");
                
            }
            
        }
        
        static bool WantsToQuit()
        {
            
            if(GameManager.Instance == null)
            {
                
                OnQuitConfirm?.Invoke();
                Application.wantsToQuit -= WantsToQuit;
                Application.Quit();

                return true;

            }
            
            if(GameManager.Instance == null || 
               GameManager.Instance.UseQuitSystemProperty == null || 
               GameManager.Instance.UseQuitSystemProperty.Value == null)
            {

                GameManager.Instance.QuitConfirm();
                
                return true;
                
            }

            if (GameManager.Instance.UseQuitSystemProperty.Value == Boolean.TrueString)
            {
                
                //ActivateQuitUI
                OnTryingToQuit?.Invoke();

                return false;
                
            }
            else
            {

                GameManager.Instance.QuitConfirm();
                
                return true;
                
            }


        }

        [RuntimeInitializeOnLoadMethod]
        static void RunOnStart()
        {
            Application.wantsToQuit += WantsToQuit;
        }

        public void TryingToQuit()
        {

            OnTryingToQuit?.Invoke();

        }

        public void QuitConfirm()
        {

            // When pressing Confirmation-Button, unsubscribe from Event and the Quit Application
            OnQuitConfirm?.Invoke();
            Application.wantsToQuit -= WantsToQuit;
            Application.Quit();

        }

    }
}