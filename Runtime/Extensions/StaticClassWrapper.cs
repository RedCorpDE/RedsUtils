using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

namespace RedsUtils
{
    public class StaticClassWrapper : MonoBehaviour
    {
        [Tooltip("Full name of the static class including namespace, e.g., MyNamespace.MyStaticClass")]
        public string className;

        [Tooltip("Name of the static field or property")]
        public string memberName;

        private Type targetType;
        private FieldInfo fieldInfo;
        private PropertyInfo propertyInfo;

        private void Awake()
        {
            if (string.IsNullOrEmpty(className) || string.IsNullOrEmpty(memberName))
            {
                Debug.LogWarning("Class name or member name is empty.");
                return;
            }

            targetType = Type.GetType(className);
            if (targetType == null)
            {
                Debug.LogError($"Static class '{className}' not found.");
                return;
            }

            fieldInfo = targetType.GetField(memberName, BindingFlags.Static | BindingFlags.Public);
            if (fieldInfo == null)
            {
                propertyInfo = targetType.GetProperty(memberName, BindingFlags.Static | BindingFlags.Public);
            }

            if (fieldInfo == null && propertyInfo == null)
            {
                Debug.LogError($"Static member '{memberName}' not found in class '{className}'.");
            }
        }

        // Generic get/set
        public object GetValue()
        {
            if (fieldInfo != null) return fieldInfo.GetValue(null);
            if (propertyInfo != null) return propertyInfo.GetValue(null);
            Debug.LogError("No valid static member found to get value.");
            return null;
        }

        public void SetValue(object value)
        {
            try
            {
                if (fieldInfo != null)
                    fieldInfo.SetValue(null, Convert.ChangeType(value, fieldInfo.FieldType));
                else if (propertyInfo != null && propertyInfo.CanWrite)
                    propertyInfo.SetValue(null, Convert.ChangeType(value, propertyInfo.PropertyType));
                else
                    Debug.LogError("No writable static member found to set value.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to set value: {ex.Message}");
            }
        }

        // UnityEvent-friendly methods
        public void SetInt(int value) => SetValue(value);
        public void SetFloat(float value) => SetValue(value);
        public void SetString(string value) => SetValue(value);
        public void SetBool(bool value) => SetValue(value);

        public int GetInt() => (int)GetValue();
        public float GetFloat() => (float)GetValue();
        public string GetString() => (string)GetValue();
        public bool GetBool() => (bool)GetValue();
    }
}