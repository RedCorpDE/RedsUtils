using System;
using System.Collections.Generic;
using UnityEngine;

namespace RedsUtils.Player.MovementSystem
{
    [CreateAssetMenu(menuName = "RedsUtils/Player/Locomotion Set", fileName = "LocomotionSet")]
    public sealed class LocomotionSet : ScriptableObject
    {
        public List<Entry> modules = new();

        [Serializable]
        public sealed class Entry
        {
            public LocomotionModuleBase module;
            public bool enabledByDefault = true;

            [Tooltip("Lower runs earlier; useful if you want deterministic ordering.")]
            public int order = 0;
        }
    }
}