using UnityEditor;
using UnityEngine;

namespace VRLabs.ToonyStandardRebuild.SimpleShaderInspectors
{
    public static class SSIGUIHelper
    {
        public static Rect GetControlRectForSingleLine()
        {
            const float CONTENT_EXTRA_SPACING = 2f;
            return EditorGUILayout.GetControlRect(true, CONTENT_EXTRA_SPACING + 16f /* EditorGUI.kSingleLineHeight*/, EditorStyles.layerMaskField); // Unity give use public non internal getters for indentation space thanks
        }

        public static Rect TexturePropertyWithHDRColorFixed(this MaterialEditor editor, GUIContent label, MaterialProperty textureProp, MaterialProperty colorProperty, bool showAlpha)
        {
            Rect rect = GetControlRectForSingleLine();
            editor.TexturePropertyMiniThumbnail(rect, textureProp, label.text, label.tooltip);

            if (colorProperty.type != MaterialProperty.PropType.Color)
            {
                Debug.LogError("Assuming MaterialProperty.PropType.Color (was " + colorProperty.type + ")");
                return rect;
            }

            editor.BeginAnimatedCheck(rect, colorProperty);

            int oldIndentLevel = EditorGUI.indentLevel;

            EditorGUI.indentLevel = 0;

            Rect leftRect = MaterialEditor.GetLeftAlignedFieldRect(rect);
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = colorProperty.hasMixedValue;
            Color newValue = EditorGUI.ColorField(leftRect, GUIContent.none, colorProperty.colorValue, true, showAlpha, true);
            EditorGUI.showMixedValue = false;
            if (EditorGUI.EndChangeCheck())
                colorProperty.colorValue = newValue;

            editor.EndAnimatedCheck();

            EditorGUI.indentLevel = oldIndentLevel;

            return rect;
        }
    }
}