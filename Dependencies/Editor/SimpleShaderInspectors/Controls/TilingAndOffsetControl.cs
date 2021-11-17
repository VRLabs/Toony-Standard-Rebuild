using UnityEditor;

namespace VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls
{
    public class TilingAndOffsetControl : PropertyControl
    {
        public TilingAndOffsetControl(string propertyName) : base(propertyName)
        {
        }

        protected override void ControlGUI(MaterialEditor materialEditor)
        {
            EditorGUI.BeginChangeCheck();
            materialEditor.TextureScaleOffsetProperty(Property);
            HasPropertyUpdated = EditorGUI.EndChangeCheck();
        }
    }
}