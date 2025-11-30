using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace RedsUtils.RoomSystem
{
    public class ModuleBase : MonoBehaviour
    {

        public string moduleName;
        public Sprite moduleIcon;

        public bool autoPlay = false;

        public List<TaskBase> tasks = new List<TaskBase>();

        public UnityEvent OnModuleStarted;
        public UnityEvent<ModuleResultsBase> OnModuleCompleted;
        public UnityEvent OnModuleCleanUp;

        [SerializeField, ReadOnly] ModuleResultsBase result;

        [SerializeField, ReadOnly] float timer;

        [SerializeField, ReadOnly] bool timerRunning = false;

        [SerializeField, ReadOnly] int currentTaskIndex = -1;

        bool isCurrentlySwitching = false;

        [SerializeField, ReadOnly] public bool isFinished = false;


        public virtual void Start()
        {
            Debug.Log("[ModuleBase] Start");

            result = ScriptableObject.CreateInstance<ModuleResultsBase>();

            for (int i = 0; i < tasks.Count; i++)
            {

                tasks[i].OnTaskCleanUp?.Invoke();

                tasks[i].OnTaskStarted.AddListener(delegate { timerRunning = true; });

                tasks[i].OnTaskCompleted.AddListener(delegate { timerRunning = false; });

                tasks[i].OnTaskCompleted.AddListener(delegate
                {

                    if (autoPlay)
                        SwitchToNextTask();

                });

                tasks[i].OnTaskCompleted.AddListener(ModuleAddResults);


            }

            isFinished = false;



        }

        private void FixedUpdate()
        {

            if (timerRunning)
            {

                timer += Time.deltaTime;

            }

        }

        [Button]
        public virtual void ModuleStarted()
        {

            OnModuleStarted?.Invoke();

        }

        public virtual void ModuleAddResults(List<string> newResults)
        {
            result.resultValues.AddRange(newResults);

        }

        [Button]
        public virtual void ModuleCompleted()
        {
            Debug.Log("[ModuleBase] Module Completed");
            result.timeElapsed = timer;
            isFinished = true;

            OnModuleCompleted?.Invoke(result);

            //Update DB

            //VfxManager.Instance?.SpawnNewParticle("Confetti",
            //    GuideTabletClient.GuideTabletClient.Instance.transform.position + new Vector3(0, 0.5f, .5f));
            //VfxManager.Instance?.SpawnNewParticle("ScalingOctagon", new Vector3(0f, .05f, 0f));

        }

        public virtual void ModuleQuit()
        {

            timer = 0;

            currentTaskIndex = -1;

            ModuleSwitchTask(currentTaskIndex);

            OnModuleCleanUp?.Invoke();

        }

        public void SwitchToNextTask()
        {
            Debug.Log($"[ModuleBase] SwitchToNextTask - current before switching {currentTaskIndex}");
            if (isCurrentlySwitching)
                return;

            if (currentTaskIndex >= 0)
                tasks[currentTaskIndex]?.OnTaskCleanUp?.Invoke();

            currentTaskIndex++;

            isCurrentlySwitching = true;

            ModuleSwitchTask(currentTaskIndex);

        }

        public virtual async void ModuleSwitchTask(int newTaskIndex)
        {

            Debug.Log($"[ModuleBase] {this.name} is trying to switch to Task {newTaskIndex}");

            await Task.Delay(2000);

            if (currentTaskIndex < tasks.Count)
            {

                TaskBase newTask = null;

                for (int i = 0; i < tasks.Count; i++)
                {

                    if (i == newTaskIndex)
                    {
                        //tasks[i].gameObject.SetActive(true);
                        newTask = tasks[newTaskIndex];
                    }
                    //else
                    //    tasks[i].gameObject.SetActive(false);

                }

                if (newTask != null)
                    newTask?.TaskStarted();
                else
                    Debug.Log($"[Module]{this.name} tried starting an unknown Task!");

            }
            else
            {

                ModuleCompleted();

            }

            isCurrentlySwitching = false;

        }

    }
}