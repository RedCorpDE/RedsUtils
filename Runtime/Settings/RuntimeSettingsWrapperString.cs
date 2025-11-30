namespace RedsUtils
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEngine;
    using UnityEngine.Events;

    public class RuntimeSettingsWrapperString : RuntimeSettingsWrapper
    {

        public UnityEvent<string> OnValueChangedExternal;

        public UnityEvent<string> OnValueChangedLocal;


        public override void Start()
        {

            base.Start();

        }

        public override void OnValueChanged(RuntimeSettingsPropertyBase property)
        {

            base.OnValueChanged(property);

            OnValueChangedLocal?.Invoke(property.name);

        }

        public override void OnValueChangedExternally(string value)
        {

            base.OnValueChangedExternally(value);

            OnValueChangedExternal?.Invoke(value);

        }

        public override void UpdateValue(string value)
        {

            base.UpdateValue(value);

        }

    }
}