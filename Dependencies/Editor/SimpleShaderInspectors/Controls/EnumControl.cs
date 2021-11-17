using System;
using UnityEditor;
using UnityEngine;

namespace VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls
{
    public class EnumControl<TEnum> : PropertyControl where TEnum : Enum
    {
        private readonly GUIContent[] _options;
        
        public TEnum SelectedOption => (TEnum)Enum.ToObject(typeof(TEnum) , Property.floatValue);

        public EnumControl(string propertyName) : base(propertyName)
        {
            string[] op = Enum.GetNames(typeof(TEnum));
            _options = new GUIContent[op.Length];
            for (int i = 0; i < op.Length; i++)
            {
                _options[i] = new GUIContent(op[i]);
            }
        }

        protected override void ControlGUI(MaterialEditor materialEditor)
        {
            int selected = (int)Property.floatValue;
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = Property.hasMixedValue;
            selected = EditorGUILayout.Popup(Content, selected, _options);
            EditorGUI.showMixedValue = false;
            HasPropertyUpdated = EditorGUI.EndChangeCheck();
            if (HasPropertyUpdated)
            {
                materialEditor.RegisterPropertyChangeUndo(Content.text);
                Property.floatValue = selected;
            }
        }
    }
}