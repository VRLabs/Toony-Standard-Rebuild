using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VRLabs.ToonyStandardRebuild.ModularShaderSystem;
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
    
    public class ListSelectorControlUIElement : VisualElement
    {
        public ListSelectorControlUIElement(List<object> parameters)
        {
            var parameterNameField = new TextField("Parameter name");
            var elementsList = new ObjectInspectorList<ListSelectorControl.ListSelectorItem>("Elements", ListSelectorItemUIElement.ElementTemplate);

            if (parameters.Count != 2)
            {
                parameters.Clear();
                parameters.Add("");
                parameters.Add(new List<ListSelectorControl.ListSelectorItem>());
            }
            else
            {
                if (!(parameters[0] is string))
                    parameters[0] = "";
                if (!(parameters[1] is List<ListSelectorControl.ListSelectorItem>))
                    parameters[1] = new List<ListSelectorControl.ListSelectorItem>();
            }

            parameterNameField.SetValueWithoutNotify((string)parameters[0]);
            elementsList.Items = (List<ListSelectorControl.ListSelectorItem>)parameters[1];
            parameterNameField.RegisterValueChangedCallback(e => parameters[0] = e.newValue);

            Add(parameterNameField);
            Add(elementsList);
        }
    }
    
    public class ListSelectorControlInstancer : IControlInstancer
    {
        public Type InstanceType => typeof(ListSelectorControl);
        public bool CanHaveChildControls => false;
        
        public SimpleControl InstanceInspectorControl(ControlUI uiAsset, IControlContainer parentControl, ModularShader shader, out string uvSet)
        {
            uvSet = null;
            if (uiAsset.Parameters.Count < 2 || !(uiAsset.Parameters[0] is string) ||
                !(uiAsset.Parameters[1] is List<ListSelectorControl.ListSelectorItem>))
                throw new TypeAccessException("The parameter given was not of the right type");
            return parentControl.AddListSelectorControl((string)uiAsset.Parameters[0], (List<ListSelectorControl.ListSelectorItem>)uiAsset.Parameters[1],
                uiAsset.AppendAfter).WithAlias(uiAsset.Name);
        }
        
        public VisualElement InstanceEditorUI(ControlUI uiAsset) => new ListSelectorControlUIElement(uiAsset.Parameters);
    }
    
}