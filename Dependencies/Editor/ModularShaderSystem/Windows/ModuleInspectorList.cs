using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace VRLabs.ToonyStandardRebuild.ModularShaderSystem
{

    public class ModuleInspectorList : BindableElement
    {
        Foldout _listContainer;
        Button _addButton;
        SerializedProperty _array;
        private bool _showElementsButtons;
        private List<string> _loadedModules;

        private bool _hasFoldingBeenForced;

        public ModuleInspectorList()
        {
            _listContainer = new Foldout();
            _listContainer.text = "Unbound List";
            _listContainer.contentContainer.AddToClassList("inspector-list-container");
            _listContainer.value = false;
            _addButton = new Button(AddItem);
            _addButton.text = "Add";
            _addButton.AddToClassList("inspector-list-add-button");
            Add(_listContainer);
            if(enabledSelf)
                _listContainer.Add(_addButton);
            _listContainer.RegisterValueChangedCallback((e) => _array.isExpanded = e.newValue);
            var styleSheet = Resources.Load<StyleSheet>(MSSConstants.RESOURCES_FOLDER + "/MSSUIElements/ModuleInspectorList");
            styleSheets.Add(styleSheet);
        }
     
        public override void HandleEvent(EventBase evt)
        {
            var type = evt.GetType(); 
            if ((type.Name == "SerializedPropertyBindEvent") && !string.IsNullOrWhiteSpace(bindingPath))
            {
                var obj = type.GetProperty("bindProperty")?.GetValue(evt) as SerializedProperty;
                _array = obj;
                if (obj != null)
                {
                    if (_hasFoldingBeenForced) obj.isExpanded = _listContainer.value;
                    else _listContainer.value = obj.isExpanded;
                }
                UpdateList();
            }
            base.HandleEvent(evt);
        }
     
        public void UpdateList()
        {
            _listContainer.Clear();
           
            if (_array == null)
                return;
            _listContainer.text = _array.displayName;

            _loadedModules = new List<string>();
            for (int i = 0; i < _array.arraySize; i++)
            {
                if(_array.GetArrayElementAtIndex(i).objectReferenceValue != null)
                    _loadedModules.Add(((ShaderModule)_array.GetArrayElementAtIndex(i).objectReferenceValue)?.Id);
            }
                
            

            for (int i = 0; i < _array.arraySize; i++)
            {
                int index = i;

                var moduleItem = new VisualElement();
                var objectField = new ObjectField();

                SerializedProperty propertyValue = _array.GetArrayElementAtIndex(index);
                
                objectField.objectType = typeof(ShaderModule);
                objectField.bindingPath = propertyValue.propertyPath;
                objectField.Bind(propertyValue.serializedObject);
                var infoLabel = new Label();
                moduleItem.Add(objectField);
                moduleItem.Add(infoLabel);

                objectField.RegisterCallback<ChangeEvent<Object>>(x =>
                {
                    var newValue = (ShaderModule)x.newValue;
                    var oldValue = (ShaderModule)x.previousValue;

                    if(oldValue != null)
                        _loadedModules.Remove(oldValue.Id);
                    if(newValue != null)
                        _loadedModules.Add(newValue.Id);

                    for (int j = 0; j < _array.arraySize; j++)
                    {
                        var field = ((ObjectField)x.target).parent.parent.parent.ElementAt(j).ElementAt(0);
                        Label label = field.ElementAt(1) as Label; 
                        if(index == j)
                            CheckModuleValidity(newValue, label, field);
                        else
                            CheckModuleValidity((ShaderModule)_array.GetArrayElementAtIndex(j).objectReferenceValue, label, field);
                    }
                });
                    
                var item = new InspectorListItem(moduleItem, _array, index, _showElementsButtons);
                item.removeButton.RegisterCallback<PointerUpEvent>((evt) => 
                {
                    RemoveItem(index);
                });
                item.upButton.RegisterCallback<PointerUpEvent>((evt) =>
                {
                    MoveUpItem(index);
                });
                item.downButton.RegisterCallback<PointerUpEvent>((evt) => 
                {
                    MoveDownItem(index);
                });
                _listContainer.Add(item);
                
                CheckModuleValidity((ShaderModule)propertyValue.objectReferenceValue, infoLabel, moduleItem);
            }
            if(enabledSelf)
                _listContainer.Add(_addButton);
        }

        private void CheckModuleValidity(ShaderModule newValue, Label infoLabel, VisualElement moduleItem)
        {
            
            List<string> problems = new List<string>();

            if (newValue != null)
            {
                var moduleId = newValue.Id;
                if (_loadedModules.Count(y => y.Equals(moduleId)) > 1)
                    problems.Add("The module is duplicate");

                List<string> missingDependencies = newValue.ModuleDependencies.Where(dependency => _loadedModules.Count(y => y.Equals(dependency)) == 0).ToList();
                List<string> incompatibilities = newValue.IncompatibleWith.Where(dependency => _loadedModules.Count(y => y.Equals(dependency)) > 0).ToList();

                if (missingDependencies.Count > 0)
                    problems.Add("Missing dependencies: " + string.Join(", ", missingDependencies));

                if (incompatibilities.Count > 0)
                    problems.Add("These incompatible modules are installed: " + string.Join(", ", incompatibilities));
            }
            
            infoLabel.text = string.Join("\n", problems);

            if (!string.IsNullOrWhiteSpace(infoLabel.text))
            {
                moduleItem.AddToClassList("error-background");
                infoLabel.visible = true;
            }
            else
            {
                moduleItem.RemoveFromClassList("error-background");
                infoLabel.visible = false;
            }
        }

        public void RemoveItem(int index)
        {
            if(_array != null)
            {
                if(index < _array.arraySize - 1)
                    _array.GetArrayElementAtIndex(index).isExpanded = _array.GetArrayElementAtIndex(index + 1).isExpanded;
                var elementProperty = _array.GetArrayElementAtIndex(index);
                if (elementProperty.objectReferenceValue != null)
                    elementProperty.objectReferenceValue = null;
                _array.DeleteArrayElementAtIndex(index);
                _array.serializedObject.ApplyModifiedProperties();
            }
     
            UpdateList();
        }
        
        public void MoveUpItem(int index)
        {
            if(_array != null && index > 0)
            {
                _array.MoveArrayElement(index, index - 1);
                bool expanded = _array.GetArrayElementAtIndex(index).isExpanded;
                _array.GetArrayElementAtIndex(index).isExpanded = _array.GetArrayElementAtIndex(index - 1).isExpanded;
                _array.GetArrayElementAtIndex(index - 1).isExpanded = expanded;
                _array.serializedObject.ApplyModifiedProperties();
            }
     
            UpdateList();
        }
        
        public void MoveDownItem(int index)
        {
            if(_array != null && index<_array.arraySize - 1)
            {
                _array.MoveArrayElement(index, index + 1);
                bool expanded = _array.GetArrayElementAtIndex(index).isExpanded;
                _array.GetArrayElementAtIndex(index).isExpanded = _array.GetArrayElementAtIndex(index + 1).isExpanded;
                _array.GetArrayElementAtIndex(index + 1).isExpanded = expanded;
                _array.serializedObject.ApplyModifiedProperties();
            }
     
            UpdateList();
        }
     
        public void AddItem()
        {
            if (_array != null)
            {
                _array.InsertArrayElementAtIndex(_array.arraySize);
                _array.serializedObject.ApplyModifiedProperties();
            }
     
            UpdateList();
        }

        public void SetFoldingState(bool open)
        {
            _listContainer.value = open;
            if (_array != null) _array.isExpanded = open;
            else _hasFoldingBeenForced = true;
        }
     
        public new class UxmlFactory : UxmlFactory<ModuleInspectorList, UxmlTraits> { }
     
        public new class UxmlTraits : BindableElement.UxmlTraits
        {
            UxmlBoolAttributeDescription showElements =
                new UxmlBoolAttributeDescription { name = "show-elements-text", defaultValue = true };
            
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                if (ve is ModuleInspectorList ate) ate._showElementsButtons = showElements.GetValueFromBag(bag, cc);
            }
        }
    }
}