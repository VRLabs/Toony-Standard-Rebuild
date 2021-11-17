using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace VRLabs.ToonyStandardRebuild.SimpleShaderInspectors
{
    public static class Localization
    {
        public static void ApplyLocalization(this IEnumerable<SimpleControl> controls, string localizationFilePath, bool writeIfNotFound = false, bool recursive = true)
        {
            LocalizationFile localizationFile;

            if (File.Exists(localizationFilePath))
                localizationFile = JsonUtility.FromJson<LocalizationFile>(File.ReadAllText(localizationFilePath));
            else
                localizationFile = new LocalizationFile();

            List<PropertyInfo> missingInfo = SetPropertiesLocalization(controls, localizationFile.Properties, recursive).ToList();

            if (missingInfo.Count > 0 && writeIfNotFound)
            {
                missingInfo.AddRange(localizationFile.Properties);
                localizationFile.Properties = missingInfo.ToArray();
                File.WriteAllText(localizationFilePath, JsonUtility.ToJson(localizationFile, true));
            }
        }
        
        public static void ApplyLocalization(this SimpleControl control, string localizationFilePath, bool writeIfNotFound = false, bool recursive = false)
        {
            LocalizationFile localizationFile;

            if (File.Exists(localizationFilePath))
                localizationFile = JsonUtility.FromJson<LocalizationFile>(File.ReadAllText(localizationFilePath));
            else
                localizationFile = new LocalizationFile();

            List<PropertyInfo> missingInfo = SetPropertiesLocalization(new []{control}, localizationFile.Properties, recursive).ToList();

            if (missingInfo.Count > 0 && writeIfNotFound)
            {
                missingInfo.AddRange(localizationFile.Properties);
                localizationFile.Properties = missingInfo.ToArray();
                File.WriteAllText(localizationFilePath, JsonUtility.ToJson(localizationFile, true));
            }
        }

        private static IEnumerable<PropertyInfo> SetPropertiesLocalization(IEnumerable<SimpleControl> controls, PropertyInfo[] propertyInfos, bool recursive = true)
        {
            List<PropertyInfo> missingInfo = new List<PropertyInfo>();
            foreach (var control in controls)
            {
                var selectedInfo = propertyInfos.FindPropertyByName(control.ControlAlias) ?? missingInfo.FindPropertyByName(control.ControlAlias);

                if (selectedInfo == null)
                {
                    selectedInfo = new PropertyInfo
                    {
                        Name = control.ControlAlias,
                        DisplayName = control.ControlAlias,
                        Tooltip = ""
                    };

                    if (!string.IsNullOrWhiteSpace(selectedInfo.Name))
                        missingInfo.Add(selectedInfo);
                }
                control.Content = new GUIContent(selectedInfo.DisplayName, selectedInfo.Tooltip);
                
                    if(control is IAdditionalLocalization additional)
                        foreach (var content in additional.AdditionalContent)
                        {
                            string fullName = control.ControlAlias + "_" + content.Name;
                            var extraInfo = propertyInfos.FindPropertyByName(fullName);
                            if (extraInfo == null)
                            {
                                extraInfo = new PropertyInfo
                                {
                                    Name = fullName,
                                    DisplayName = fullName,
                                    Tooltip = ""
                                };
                                if (!string.IsNullOrWhiteSpace(extraInfo.Name))
                                    missingInfo.Add(extraInfo);
                            }

                            content.Content = new GUIContent(extraInfo.DisplayName, extraInfo.Tooltip);
                        }

                    if(control is IControlContainer container)
                        if (recursive) missingInfo.AddRange(SetPropertiesLocalization(container.GetControlList(), propertyInfos));
                
            }
            return missingInfo;
        }
    }

    public static class LocalizationSearchers
    {
        public static PropertyInfo FindPropertyByName(this IEnumerable<PropertyInfo> properties, string name)
        {
            return properties?.FirstOrDefault(property => property.Name.Equals(name));
        }
        public static string FindPropertyByName(this (string, string)[] properties, string name)
        {
            for (int i = 0; i < properties.Length; i++)
            {
                if (properties[i].Item1.Equals(name))
                {
                    return properties[i].Item2;
                }
            }
            return null;
        }
    }

    [Serializable]
    public class PropertyInfo
    {
        public string Name;
        public string DisplayName;
        public string Tooltip;
    }
    [Serializable]
    public class LocalizationFile
    {
        public PropertyInfo[] Properties;
        
        public LocalizationFile()
        {
            Properties = Array.Empty<PropertyInfo>();
        }
    }
    [Serializable]
    public class SettingsFile
    {
        public string SelectedLanguage;
    }
}