using UnityEditor;
using UnityEngine;
namespace VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls
{
    public class LightmapEmissionControl : SimpleControl
    {
        public LightmapEmissionControl() : base("") { }

        public bool HasLightmapEmissionUpdated { get; protected set; }

        protected override void ControlGUI(MaterialEditor materialEditor)
        {
            EditorGUI.BeginChangeCheck();
            materialEditor.LightmapEmissionProperty();
            HasLightmapEmissionUpdated = EditorGUI.EndChangeCheck();
            if (HasLightmapEmissionUpdated)
            {
                foreach (Material mat in Inspector.Materials)
                {
                    MaterialEditor.FixupEmissiveFlag(mat);
                    bool shouldEmissionBeEnabled = (mat.globalIlluminationFlags & MaterialGlobalIlluminationFlags.EmissiveIsBlack) == 0;
                    mat.SetOverrideTag("IsEmissive", shouldEmissionBeEnabled ? "true" : "false");
                }
            }
        }
    }
}