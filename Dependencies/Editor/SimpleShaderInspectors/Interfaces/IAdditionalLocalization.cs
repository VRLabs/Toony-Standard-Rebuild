using System;
using UnityEngine;

namespace VRLabs.ToonyStandardRebuild.SimpleShaderInspectors
{
    public interface IAdditionalLocalization
    {
        AdditionalLocalization[] AdditionalContent { get; set; }
    }

    public class AdditionalLocalization : IEquatable<AdditionalLocalization>
    {
        public string Name { get; set; }
        
        public GUIContent Content { get; set; }

        public bool Equals(AdditionalLocalization other)
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
            return Equals((AdditionalLocalization)obj);
        }

        public override int GetHashCode() => Name != null ? Name.GetHashCode() : 0;
    }

    public static class AdditionalContentExtensions
    {
        public static void InitializeLocalizationWithNames(this IAdditionalLocalization obj, string[] contentNames)
        {
            obj.AdditionalContent = new AdditionalLocalization[contentNames.Length];
            for (int i = 0; i < contentNames.Length; i++)
                obj.AdditionalContent[i] = new AdditionalLocalization { Name = contentNames[i] };

        }
        public static AdditionalLocalization[] CreateLocalizationArrayFromNames(string[] contentNames)
        {
            AdditionalLocalization[] obj = new AdditionalLocalization[contentNames.Length];
            for (int i = 0; i < contentNames.Length; i++)
                obj[i] = new AdditionalLocalization { Name = contentNames[i] };
            
            return obj;
        }
    }
}