using UnityEditor;

namespace VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls
{
    public class DoubleSidedGIControl : SimpleControl
    {
        public DoubleSidedGIControl() : base("") { }

        public bool HasDoubleSidedGIUpdated { get; protected set; }

        protected override void ControlGUI(MaterialEditor materialEditor)
        {
            EditorGUI.BeginChangeCheck();
            materialEditor.DoubleSidedGIField();
            HasDoubleSidedGIUpdated = EditorGUI.EndChangeCheck();
        }
    }
}