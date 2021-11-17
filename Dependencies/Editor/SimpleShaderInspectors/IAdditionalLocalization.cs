using UnityEngine;

namespace VRLabs.ToonyStandardRebuild.SimpleShaderInspectors
{
    public interface IAdditionalLocalization
    {
        AdditionalLocalization[] AdditionalContent { get; set; }
    }

    public class AdditionalLocalization
    {
        public string Name { get; set; }
        
        public GUIContent Content { get; set; }
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