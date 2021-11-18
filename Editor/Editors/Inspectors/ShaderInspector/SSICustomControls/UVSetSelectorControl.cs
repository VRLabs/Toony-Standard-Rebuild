using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
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
}