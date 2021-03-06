using UnityEditor;

namespace VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls
{
    public class HelpBoxControl : SimpleControl
    {
        [FluentSet] public MessageType BoxType { get; set; }
        
        [FluentSet] public bool IsWideBox { get; set; }
        
        public HelpBoxControl(string alias) : base(alias)
        {
            BoxType = MessageType.None;
            IsWideBox = true;
        }

        protected override void ControlGUI(MaterialEditor materialEditor)
        {
            EditorGUILayout.HelpBox(Content.text, BoxType, IsWideBox);
        }
    }
}