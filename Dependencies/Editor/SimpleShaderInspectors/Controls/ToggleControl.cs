using System;
using UnityEditor;

namespace VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls
{
    public class ToggleControl : PropertyControl
    {
        protected readonly float FalseValue;

        protected readonly float TrueValue;

        public bool ToggleEnabled => Math.Abs((Property?.floatValue ?? 0) - TrueValue) < 0.001;

        public ToggleControl(string propertyName, float falseValue = 0, float trueValue = 1) : base(propertyName)
        {
            this.FalseValue = falseValue;
            this.TrueValue = trueValue;
        }

        protected override void ControlGUI(MaterialEditor materialEditor)
        {
            EditorGUI.showMixedValue = Property.hasMixedValue;

            EditorGUI.BeginChangeCheck();
            bool toggle = EditorGUILayout.Toggle(Content, ToggleEnabled);
            HasPropertyUpdated = EditorGUI.EndChangeCheck();
            if (HasPropertyUpdated)
            {
                materialEditor.RegisterPropertyChangeUndo(Property.displayName);
                Property.floatValue = toggle ? TrueValue : FalseValue;
            }

            EditorGUI.showMixedValue = false;
        }
    }
}