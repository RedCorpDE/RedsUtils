namespace RedsUtils
{
    using UnityEngine.Events;

    public class RuntimeSettingsWrapperBool : RuntimeSettingsWrapper
    {

        public UnityEvent OnValueChangedExternallyToActive;
        public UnityEvent OnValueChangedExternallyToInactive;

        public UnityEvent OnValueChangedToActive;
        public UnityEvent OnValueChangedToInactive;

        public string trueString;
        public string falseString;


        public override void Start()
        {

            base.Start();

        }

        public override void OnValueChanged(RuntimeSettingsPropertyBase property)
        {
            base.OnValueChanged(property);

            if (property.Value.ToLower() == trueString.ToLower())
                OnValueChangedToActive.Invoke();
            else if (property.Value.ToLower() == falseString.ToLower())
                OnValueChangedToInactive.Invoke();

        }

        public override void OnValueChangedExternally(string value)
        {

            base.OnValueChangedExternally(value);

            if (value.ToLower() == trueString.ToLower())
                OnValueChangedExternallyToActive.Invoke();
            else if (value.ToLower() == falseString.ToLower())
                OnValueChangedExternallyToInactive.Invoke();

        }

        public override void UpdateValue(string value)
        {

            base.UpdateValue(value);

        }

    }
}