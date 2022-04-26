using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VRLabs.ToonyStandardRebuild.ModularShaderSystem;
using VRLabs.ToonyStandardRebuild.SimpleShaderInspectors;

namespace VRLabs.ToonyStandardRebuild.SSICustomControls
{
    public class UVSetSelectorControl : PropertyControl
    {
        public class UvSetSelectorItem
        {
            public string Name;
            public int Index;

        }
        private GUIContent[] _options;
        private int _previousIndex = -1;

        public int SelectedOption =>  (int)Property.floatValue;

        private List<string> _items;

        public UVSetSelectorControl(string propertyName, List<string> items) : base(propertyName)
        {
            _items = items;
            _options = new GUIContent[_items.Count];
            for (int i = 0; i < _items.Count; i++)
            {
                _options[i] = new GUIContent(_items[i]);
            }
        }

        public void SetNewOptions(List<string> items)
        {
            _items = items;
            _options = new GUIContent[_items.Count];
            for (int i = 0; i < _items.Count; i++)
            {
                _options[i] = new GUIContent(_items[i]);
            }
        }
        
        protected override void ControlGUI(MaterialEditor materialEditor)
        {
            int selected = _previousIndex;
            if(_previousIndex != (int)Property.floatValue || selected == -1)  selected = (int)Property.floatValue;
            if (_options.Length <= selected) Property.floatValue = selected = 0;
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = Property.hasMixedValue;
            selected = EditorGUILayout.Popup(Content, selected, _options);
            EditorGUI.showMixedValue = false;
            HasPropertyUpdated = EditorGUI.EndChangeCheck();
            if (HasPropertyUpdated)
            {
                materialEditor.RegisterPropertyChangeUndo(Content.text);
                _previousIndex = selected;
                Property.floatValue = selected;
            }
        }
    }
    
    public class UVSetSelectorControlUIElement : VisualElement
    {
        public UVSetSelectorControlUIElement(List<object> parameters)
        {
            var parameterNameField = new TextField("Parameter name");
            var uvSetNameField = new TextField("UV Set");

            if (parameters.Count != 2)
            {
                parameters.Clear();
                parameters.Add("");
                parameters.Add("");
            }
            else
            {
                if (!(parameters[0] is string))
                    parameters[0] = "";
                if (!(parameters[1] is string))
                    parameters[1] = "";
            }

            parameterNameField.SetValueWithoutNotify((string)parameters[0]);
            parameterNameField.RegisterValueChangedCallback(e => parameters[0] = e.newValue);
            uvSetNameField.SetValueWithoutNotify((string)parameters[1]);
            uvSetNameField.RegisterValueChangedCallback(e => parameters[1] = e.newValue);

            Add(parameterNameField);
            Add(uvSetNameField);
        }
    }
    
    public class UVSetSelectorControlInstancer : IControlInstancer
    {
        public Type InstanceType => typeof(UVSetSelectorControl);
        public bool CanHaveChildControls => false;
        
        public SimpleControl InstanceInspectorControl(ControlUI uiAsset, IControlContainer parentControl, ModularShader shader, out string uvSet)
        {
            
            if (uiAsset.Parameters.Count < 2 || !(uiAsset.Parameters[0] is string) || !(uiAsset.Parameters[1] is string))
                throw new TypeAccessException("The parameter given was not of the right type");
            if(string.IsNullOrWhiteSpace((string)uiAsset.Parameters[1]))
                throw new TypeAccessException("UV set cannot be empty");
            uvSet = (string)uiAsset.Parameters[1];
            return parentControl.AddUVSetSelectorControl((string)uiAsset.Parameters[0], new List<string>(new[] { "uv0" })).WithAlias(uiAsset.Name);
        }
        
        public VisualElement InstanceEditorUI(ControlUI uiAsset) => new UVSetSelectorControlUIElement(uiAsset.Parameters);
    }
}