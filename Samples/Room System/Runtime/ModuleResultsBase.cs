using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedsUtils.RoomSystem
{
    [CreateAssetMenu(fileName = "", menuName = "Modules/Results/Basic")]

    public class ModuleResultsBase : ScriptableObject
    {

        [MinValue(0)] public float timeElapsed = 0;

        public List<string> resultValues = new List<string>();

        [Button]
        public virtual void CalculateResults(out List<string> results)
        {

            results = new List<string>();

            results.Add(timeElapsed.ToString());
            results.AddRange(resultValues);

            Debug.Log($"[Results] {results} {results.Count}");

        }

    }
}