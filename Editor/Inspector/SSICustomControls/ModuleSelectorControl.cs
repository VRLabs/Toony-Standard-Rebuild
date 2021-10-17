using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRLabs.ToonyStandardRebuild.ModularShaderSystem;
using VRLabs.ToonyStandardRebuild.SimpleShaderInspectors;

namespace VRLabs.ToonyStandardRebuild.SSICustomControls
{
    public class ModuleSelectorControl : PropertyControl
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
        private int _previousValue;

        public int SelectedOption =>  (int)Property.floatValue;

        public ModuleSelectorControl(string propertyName, ModularShader shader) : base(propertyName)
        {
            List<ShaderModule> items = shader.BaseModules.Concat(shader.AdditionalModules).Where(x => x.Enabled?.Name.Equals(propertyName) ?? false).ToList();
            _indexes = new List<int>();
            int start = 0;
            int count = items.Count;
            if (!items.Any(x => x.Enabled.EnableValue == 0))
            {
                count++;
                start++;
            }
            _options = new GUIContent[count];
            if (start == 1)
            {
                _options[0] = new GUIContent("None");
                _indexes.Add(0);
            }
            for (int i = 0; i < items.Count; i++)
            {
                _options[i + start] = new GUIContent(items[i].Name);
                _indexes.Add(items[i].Enabled.EnableValue);
            }  
        }
        
        protected override void ControlGUI(MaterialEditor materialEditor)
        {
            int selected = _previousIndex;
            if (_previousValue != (int)Property.floatValue || selected == -1)
            {
                int value = (int)Property.floatValue;
                if (_indexes.Any(x => x == value))
                {
                    selected = _indexes.IndexOf(value);
                }
                else
                {
                    selected = 0;
                    Property.floatValue = _indexes[0];
                }
            }
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
            }
        }
    }
}