namespace RedsUtils
{
    using System.Collections;
    using System.Collections.Generic;
    using Sirenix.OdinInspector;
    using UnityEngine;
    using UnityEngine.Localization;
    using UnityEngine.Localization.Settings;

    public class LanguageLocaleSwitchHandler : MonoBehaviour
    {

        [ReadOnly] public Locale selectedLanguage;

        public void ChangeUsedLocale(string locale)
        {

            int localeIndex = LocalizationSettings.AvailableLocales.Locales.FindIndex((x) => x.Identifier == locale);

            if (localeIndex != -1)
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[localeIndex];
            else
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[0];

            selectedLanguage = LocalizationSettings.SelectedLocale;

            Debug.Log(
                $"Using locale '{locale}' at index {localeIndex} -> selected locale '{selectedLanguage.Identifier}'");

        }

    }
}