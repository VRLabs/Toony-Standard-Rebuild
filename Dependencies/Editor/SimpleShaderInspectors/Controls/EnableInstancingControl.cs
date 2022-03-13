using UnityEditor;

namespace VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls
{
    public class EnableInstancingControl : SimpleControl
    {
        public EnableInstancingControl() : base("") { }

        public bool HasInstancingUpdated { get; protected set; }

        protected override void ControlGUI(MaterialEditor materialEditor)
        {
            EditorGUI.BeginChangeCheck();
            materialEditor.EnableInstancingField();
            HasInstancingUpdated = EditorGUI.EndChangeCheck();
        }
    }
}