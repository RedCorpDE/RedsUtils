namespace RedsUtils
{
    using Sirenix.OdinInspector;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEngine;
    using UnityEngine.Events;

    public class RuntimeSettingsWrapperStringArray : RuntimeSettingsWrapper
    {

        public Dictionary<string, UnityEvent> onValueChanged = new Dictionary<string, UnityEvent>();

        //[DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.ExpandedFoldout)]
        public Dictionary<string, UnityEvent> onValueChangedExternally = new Dictionary<string, UnityEvent>();


        public override void Start()
        {

            base.Start();

        }

        public override void OnValueChanged(RuntimeSettingsPropertyBase property)
        {

            base.OnValueChanged(property);



        }

        public override void OnValueChangedExternally(string value)
        {

            base.OnValueChangedExternally(value);

            if (onValueChangedExternally.ContainsKey(value))
                onValueChangedExternally[value].Invoke();

        }

        public override void UpdateValue(string value)
        {

            base.UpdateValue(value);

        }

    }
}