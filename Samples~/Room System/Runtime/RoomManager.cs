using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using RedsUtils;
using GPUI;

namespace RedsUtils.RoomSystem
{
    
    public class RoomManager : Singleton<RoomManager>
    {
    
        public string roomName;
        public UiSkinPalette skinPalette;
    
        [SerializeField, ReadOnly]
        int currentModuleIndex = -1;
        public List<ModuleBase> modules = new List<ModuleBase>();
    
        public UnityEvent OnLevelLoaded;
        public UnityEvent OnRoomEntered;
        public UnityEvent OnRoomCompleted;
        public UnityEvent<int> OnModuleSwitched;
        public UnityEvent<int> OnModuleCompleted;
    
        public virtual void Start()
        {
            
            OnLevelLoaded?.Invoke();
            
            //Remove this Event, if moving to the next one directly is not wished for!
            //OnModuleCompleted.AddListener(GoToNextModule);
    
            for (int i = 0; i < modules.Count; i++)
            {
    
                int moduleIndex = i;
    
                modules[moduleIndex].OnModuleStarted.AddListener(delegate { ModuleSwitched(moduleIndex); });
                modules[moduleIndex].OnModuleCompleted.AddListener(delegate { ModuleCompleted(moduleIndex); });
    
                modules[moduleIndex].OnModuleCleanUp?.Invoke();
    
            }
    
            RoomEntered();
    
        }
    
        public virtual async void RoomEntered()
        {
            
            await Task.Delay(3500);
            
            OnRoomEntered?.Invoke();
    
        }
    
        public virtual void RoomCompleted()
        {
    
            Debug.Log($"[Room] {roomName} has completed");
            OnRoomCompleted?.Invoke();
    
        }
    
        public virtual void ModuleSwitched(int index)
        {
    
            Debug.Log($"[Room] {roomName} is switching to module {index + 1}");
            OnModuleSwitched?.Invoke(index);
    
        }
    
        public virtual void ModuleCompleted(int index)
        {
            Debug.Log($"[Room] {roomName} has completed Module {modules[index].moduleName}");
    
            //UI stuff
            OnModuleCompleted?.Invoke(index);
    
        }
    
        [Button]
        public async void SwitchModuleTo(int newIndex)
        {
    
            Debug.Log($"[Room] {roomName} is trying to switch to module {newIndex + 1}");
    
            ModuleBase newModule = null;
    
            for(int i = 0; i < modules.Count; i++)
            {
    
                if (i == newIndex)
                    newModule = modules[i];
                
                modules[i].ModuleQuit();
    
            }
    
            await Task.Delay(500);
            
            Debug.Log($"[Room] {roomName} will switch to Module {newModule.moduleName}/{newIndex + 1}");
    
            newModule?.ModuleStarted();
    
        }
    
        void GoToNextModule(int currentIndex)
        {
    
    
            int newIndex = currentIndex + 1;
    
            if (newIndex < modules.Count)
            {
    
                modules[newIndex].ModuleStarted();
    
            }
            else
            {
    
                newIndex = modules.IndexOf(modules.Find((x)=> x.isFinished == false));
    
                if(newIndex == -1)
                    RoomCompleted();
                else
                    modules[newIndex].ModuleStarted();
    
            }
    
    
        }
    
    }
    
}

