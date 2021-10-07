using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRLabs.ToonyStandardRebuild.SimpleShaderInspectors;

namespace VRLabs.ToonyStandardRebuild.SSICustomControls
{
    public class ListSelectorControl : PropertyControl
    {
        public class ListSelectorItem
        {
            public string Name;
            public int Index;
            public UpdateData PropsOnSelected;

        }
        private readonly GUIContent[] _options;
        private readonly List<int> _indexes;
        private int _previousIndex = -1;
        private int _previousValue = 0;

        public int SelectedOption =>  (int)Property.floatValue;

        private List<ListSelectorItem> _items;

        public ListSelectorControl(string propertyName, List<ListSelectorItem> items) : base(propertyName)
        {
            _items = items;
            _indexes = new List<int>();
            _options = new GUIContent[_items.Count];
            for (int i = 0; i < _items.Count; i++)
            {
                _options[i] = new GUIContent(_items[i].Name);
                _indexes.Add(_items[i].Index);
            }
        }
        
        protected override void ControlGUI(MaterialEditor materialEditor)
        {
            int selected = _previousIndex;
            if(_previousValue != (int)Property.floatValue || selected == -1)  selected = _indexes.IndexOf((int)Property.floatValue);
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = Property.hasMixedValue;
            selected = EditorGUILayout.Popup(Content, selected, _options);
            EditorGUI.showMixedValue = false;
            HasPropertyUpdated = EditorGUI.EndChangeCheck();
            if (HasPropertyUpdated)
            {
                materialEditor.RegisterPropertyChangeUndo(Content.text);
                _previousIndex = selected;
                _previousValue = _indexes[selected];
                Property.floatValue = _indexes[selected];

                _items[selected].PropsOnSelected.UpdateMaterials(Inspector.Materials);
            }
        }
    }
}