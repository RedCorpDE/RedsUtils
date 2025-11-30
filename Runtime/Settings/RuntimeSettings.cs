using System;
using Hextant;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Sirenix.Serialization;
using Sirenix.OdinInspector;
using NUnit.Framework.Internal;
using System;
using System.Threading.Tasks;
using UnityEngine.Android;
using UnityEngine.Localization.Settings;

#if UNITY_EDITOR
    using Hextant.Editor;
    using UnityEditor;
#endif

namespace RedsUtils
{

    [Settings(SettingsUsage.RuntimeProject, "Game Settings")]
    public sealed class RuntimeSettings : Settings<RuntimeSettings>
    {

#if UNITY_EDITOR
        [SettingsProvider]
        public static SettingsProvider GetSettingsProvider() => instance.GetSettingsProvider();
#endif


        [SerializeField] private bool usePlayerPrefs = false;

        public bool UsePlayerPrefs
        {
            get => usePlayerPrefs;
            set => usePlayerPrefs = value;
        }

        public List<RuntimeSettingsPropertyBase> properties = new List<RuntimeSettingsPropertyBase>();
        
        bool queuedSave = false;

        private void Awake()
        {

#if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageWrite);
        }

        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageRead);
        }
#endif

        }

        public void AddProperty(RuntimeSettingsPropertyBase property)
        {
            
            RuntimeSettingsPropertyBase foundProperty = GetProperty(property.name);

            if (foundProperty != null)
            {
                
                SetPropertyValue(property.name, property);
                
            }
            else
            {
                
                properties.Add(property);
                HandleSaving();
                
            }
            
        }
        
        public void AddProperty(string propertyName, string propertyValue)
        {
            
            RuntimeSettingsPropertyBase foundProperty = GetProperty(propertyName);

            if (foundProperty != null)
            {
                
                SetPropertyValue(propertyName, propertyValue);
                
            }
            else
            {

                foundProperty = new RuntimeSettingsPropertyBase()
                {
                    name = propertyName,
                    Value = propertyValue
                };
                
                properties.Add(foundProperty);
                HandleSaving();
                
            }
            
        }

        public string GetPropertyValue(string propertyName)
        {

            if (propertyName == null || string.IsNullOrWhiteSpace(propertyName))
                return string.Empty;

            RuntimeSettingsPropertyBase foundProperty = GetProperty(propertyName);

            if (foundProperty == null)
                return string.Empty;

            return foundProperty.Value.ToString();

        }

        public RuntimeSettingsPropertyBase GetProperty(string propertyName)
        {

            if (propertyName == null || string.IsNullOrWhiteSpace(propertyName))
                return null;

            RuntimeSettingsPropertyBase foundProperty = properties.Find((x) => x.name == propertyName);

            return foundProperty;

        }

        public void SetPropertyValue(string propertyName, string propertyValue)
        {

            if (propertyName == null || string.IsNullOrWhiteSpace(propertyName))
            {

                Debug.LogError($"There is no property named '{propertyName}' in settings");

                return;

            }

            RuntimeSettingsPropertyBase foundProperty = GetProperty(propertyName);

            if (foundProperty == null)
            {

                Debug.LogError($"Could not find property '{propertyName}' in settings");

                return;

            }

            foundProperty.Value = propertyValue;

            HandleSaving();

        }

        public void SetPropertyValue(string propertyName, RuntimeSettingsPropertyBase newProperty)
        {

            if (propertyName == null || string.IsNullOrWhiteSpace(propertyName))
                return;

            RuntimeSettingsPropertyBase foundProperty = GetProperty(propertyName);

            if (foundProperty == null)
                return;

            foundProperty.Value = newProperty.Value;

            HandleSaving();

        }

        private void OnEnable()
        {
            
#if UNITY_ANDROID

        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageWrite);
        }

        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageRead);
        }

        if (PlayerPrefs.HasKey("Language"))
        {
            for (int i = 0; i < properties.Count; i++)
            {
            
                properties[i] = new RuntimeSettingsPropertyBase()
                {
                    name = properties[i].name,
                    Value = PlayerPrefs.GetString(properties[i].name)
                };
            
            }
        }
        else
        {
            HandleSaving();
        }

#else

            RuntimeSettingsPropertyBase languageProperty = GetProperty("Language");

            if (languageProperty != null)
            {

                LocalizationSettings.SelectedLocale =
                    LocalizationSettings.AvailableLocales.Locales.Find((x) =>
                        x.Identifier.Code == languageProperty.Value.ToString());

            }

            if (File.Exists($"{Application.persistentDataPath}/{Application.companyName}-Settings.json"))
            {

                string jsonString = File.ReadAllText($"{Application.persistentDataPath}/{Application.companyName}-Settings.json");

                JsonUtility.FromJsonOverwrite(jsonString, RuntimeSettings.instance);

            }
            else
            {

                HandleSaving();

            }
#endif



        }

        [Button("Save Settings")]
        public void HandleSaving()
        {

#if UNITY_EDITOR

            Debug.Log("Saving Settings");

            if (usePlayerPrefs)
            {
                SaveSettingsToPlayerPrefs();
            }
            else
            {
                SaveSettingsToJson();
            }

#else
            if(queuedSave)
                return;

            Debug.Log("Saving Settings");

            if (usePlayerPrefs)
            {
                SaveSettingsToPlayerPrefs();
            }
            else
            {
                SaveSettingsToJson();
            }
            
            queuedSave = false;
#endif

        }

        static void SaveSettingsToJson()
        {

            Debug.Log("Saving Settings to Json");
            
            RuntimeSettings.instance.queuedSave = true;

            string jsonString = JsonUtility.ToJson(RuntimeSettings.instance, true);
            
            File.WriteAllText($"{Application.persistentDataPath}/{Application.companyName}-Settings.json", jsonString);

        }

        static void SaveSettingsToPlayerPrefs()
        {

            Debug.Log("Saving Settings to PlayerPrefs");
            
            RuntimeSettings.instance.queuedSave = true;
            
            PlayerPrefs.DeleteAll();

            for (int i = 0; i < RuntimeSettings.instance.properties.Count; i++)
            {

                PlayerPrefs.SetString(RuntimeSettings.instance.properties[i].name,
                    RuntimeSettings.instance.properties[i].Value.ToString());

            }

            PlayerPrefs.Save();

        }

        [Button("Update Settings")]
        public void UpdateSettings()
        {

#if !UNITY_EDITOR
        for(int i = 0; i < properties.Count; i++)
        {

            properties[i].Value = properties[i].Value;
            
            if(properties[i].name == "Language")
                LocalizationSettings.SelectedLocale =
 LocalizationSettings.AvailableLocales.Locales.Find((x)=>x.Identifier.Code == properties[i].Value.ToString());

        }
#endif

        }

    }

    [Serializable]
    public class RuntimeSettingsPropertyBase
    {

        public string name;

        [SerializeField] private string value;

        public string Value
        {

            get => value;
            set
            {
                this.value = value;
                OnPropertyChanged?.Invoke(value);
            }


        }

        public event Action<string> OnPropertyChanged;

    }

    public interface ISettingsProperty<T>
    {

        public T Value { get; set; }

    }
}