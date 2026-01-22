using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Localization;


namespace RedsUtils.RoomSystem
{
    [CreateAssetMenu(fileName = "", menuName = "Tasks/Data/New Data")]

    public class TaskDataBase : ScriptableObject
    {

        [SerializeField] private string taskName;

        public string TaskName
        {
            get { return taskName; }
            set { taskName = value; }
        }

        public LocalizedString taskDescription;

        [HideInInspector] public List<TaskDataValue<string>> taskDataValues = new List<TaskDataValue<string>>();



    }

    [Serializable]
    public class TaskDataValue<T>
    {

        public T value;

    }
}