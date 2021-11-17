using UnityEditor;
using UnityEngine;

namespace VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls
{
    public class ColorControl : PropertyControl
    {
        [Chainable] public bool ShowAlphaValue { get; set; }

        public Color SelectedColor => Property.colorValue;

        public ColorControl(string propertyName, bool showAlphaValue = true) : base(propertyName)
        {
            ShowAlphaValue = showAlphaValue;
        }

        protected override void ControlGUI(MaterialEditor materialEditor)
        {
            Color boxColor = Property.colorValue;
            EditorGUI.BeginChangeCheck();
            bool hdr = Property.flags == MaterialProperty.PropFlags.HDR;

            Rect colorPropertyRect = EditorGUILayout.GetControlRect();
            colorPropertyRect.width = EditorGUIUtility.labelWidth + EditorGUIUtility.fieldWidth;
            EditorGUI.showMixedValue = Property.hasMixedValue;
            boxColor = EditorGUI.ColorField(colorPropertyRect, Content, boxColor, true, ShowAlphaValue, hdr);

            EditorGUI.showMixedValue = false;
            HasPropertyUpdated = EditorGUI.EndChangeCheck();
            if (HasPropertyUpdated)
            {
                materialEditor.RegisterPropertyChangeUndo(Property.displayName);
                Property.colorValue = boxColor;
            }
        }
    }
}