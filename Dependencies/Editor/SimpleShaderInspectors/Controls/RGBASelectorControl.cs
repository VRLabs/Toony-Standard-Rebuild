using UnityEditor;
using UnityEngine;

namespace VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls
{
    public class RGBASelectorControl : PropertyControl
    {

        public RGBASelectorControl(string propertyName) : base(propertyName)
        {}

        protected override void ControlGUI(MaterialEditor materialEditor)
        {
            bool hasContent = Content != null && !string.IsNullOrWhiteSpace(Content.text);
            float channel = Property.floatValue;
            EditorGUI.BeginChangeCheck();
            if (hasContent)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(Content);
                channel = GUILayout.Toolbar((int)channel, new[] { "R", "G", "B", "A" }); 
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                channel = GUILayout.Toolbar((int)channel, new[] { "R", "G", "B", "A" });
            }
            HasPropertyUpdated = EditorGUI.EndChangeCheck();
            if (HasPropertyUpdated)
            {
                Property.floatValue = channel;
            }
            
        }
    }
}