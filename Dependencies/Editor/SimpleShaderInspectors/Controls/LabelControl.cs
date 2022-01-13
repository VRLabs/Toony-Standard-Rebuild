using UnityEditor;
using UnityEngine;

namespace VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls
{
    public class LabelControl : SimpleControl
    {
        [FluentSet] public GUIStyle LabelStyle { get; set; }

        public LabelControl(string alias) : base(alias)
        {
            LabelStyle = EditorStyles.label;
        }

        protected override void ControlGUI(MaterialEditor materialEditor)
        {
            EditorGUILayout.LabelField(Content, LabelStyle);
        }
    }
}