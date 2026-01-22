using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace RedsUtils.RoomSystem
{
    
    public class TaskBase : MonoBehaviour
    {

        public string taskName;

        [ReadOnly] public bool isRunning = false;

        public UnityEvent OnTaskStarted;
        public UnityEvent<List<string>> OnTaskCompleted;
        public UnityEvent OnTaskCleanUp;

        public List<string> taskResults = new List<string>();

        public virtual void TaskStarted()
        {

            isRunning = true;
        
            OnTaskStarted?.Invoke();

        }

        [Button]
        public virtual async void TaskCompleted()
        {

            if (!isRunning)
                return;
        
            isRunning = false;

            Debug.Log("[TaskBase]  TaskCompleted");
        
            Vector3 pos = Camera.main.transform.TransformPoint(Vector3.forward * 1f);
            pos += new Vector3(0f, .3f, 0f);

            //VfxManager.Instance?.SpawnNewParticle("Emoticon_ThumbsUp", pos);

            await Task.Delay(500);

            OnTaskCompleted?.Invoke(taskResults);

        }

        public virtual void AddResultToTask(string newResult)
        {

            if (string.IsNullOrEmpty(newResult) || string.IsNullOrWhiteSpace(newResult))
                return;

            Debug.Log("[TaskBase]  AddResultToTask: " + newResult + "      Task: " + taskName);
            taskResults.Add(newResult);

        }

    }
    
}