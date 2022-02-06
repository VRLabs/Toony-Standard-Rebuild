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

            List<PropertyInfo> missingInfo = SetPropertiesLocalization(controls, localizationFile.Properties, null, recursive).Distinct().ToList();

            if (missingInfo.Count > 0 && writeIfNotFound)
            {
                missingInfo.AddRange(localizationFile.Properties);
                localizationFile.Properties = missingInfo.ToArray();
                File.WriteAllText(localizationFilePath, JsonUtility.ToJson(localizationFile, true));
            }
        }

        public static void ApplyLocalization(this SimpleControl control, string localizationFilePath, bool writeIfNotFound = false, bool recursive = false) =>
            ApplyLocalization(new[] { control }, localizationFilePath, writeIfNotFound, recursive);

        internal static List<PropertyInfo> SetPropertiesLocalization(IEnumerable<SimpleControl> controls, PropertyInfo[] propertyInfos, List<PropertyInfo> missingInfo, bool recursive = true)
        {
            if(missingInfo == null) missingInfo = new List<PropertyInfo>();
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
                        if (recursive) missingInfo = SetPropertiesLocalization(container.GetControlList(), propertyInfos, missingInfo);
                
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
    public class PropertyInfo : IEquatable<PropertyInfo>
    {
        public string Name;
        public string DisplayName;
        public string Tooltip;

        public bool Equals(PropertyInfo other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Name == other.Name;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PropertyInfo)obj);
        }

        public override int GetHashCode() => (Name != null ? Name.GetHashCode() : 0);
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