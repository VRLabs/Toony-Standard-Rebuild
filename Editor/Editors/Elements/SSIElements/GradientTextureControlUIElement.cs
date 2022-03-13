using UnityEngine.UIElements;
using System.Collections.Generic;

namespace VRLabs.ToonyStandardRebuild.SSIControls
{
    public class GradientTextureControlUIElement : VisualElement
    {
        private TextField _textureNameField;
        private TextField _colorPropertyField;
        private TextField _minColorPropertyField;
        private TextField _maxColorPropertyField;
        
        private List<object> _parameters;

        public GradientTextureControlUIElement(List<object> parameters, List<ControlUI> controls)
        {
            _parameters = parameters;
            
            _textureNameField = new TextField("Texture name");
            _colorPropertyField = new TextField("Color property");
            var controlsList = new ObjectInspectorList<ControlUI>("Settings Controls", ControlsUIElement.ElementTemplate);
            controlsList.Items = controls;
            var useMinMax = new Toggle("Use min-max values");
            _minColorPropertyField = new TextField("Min Color property");
            _maxColorPropertyField = new TextField("Max Color property");
            useMinMax.value = parameters.Count == 4;

            if (parameters.Count == 2)
            {
                if (!(parameters[0] is string))
                    parameters[0] = "";
                if (!(parameters[1] is string))
                    parameters[1] = "";
            }
            else if (parameters.Count == 4)
            {
                if (!(parameters[0] is string))
                    parameters[0] = "";
                if (!(parameters[1] is string))
                    parameters[1] = "";
                if (!(parameters[2] is string))
                    parameters[2] = "";
                if (!(parameters[3] is string))
                    parameters[3] = "";
            }
            else
            {
                parameters.Clear();
                parameters.Add("");
                parameters.Add("");
            }

            if (useMinMax.value)
            {
                _textureNameField.SetValueWithoutNotify((string)parameters[0]);
                _colorPropertyField.SetValueWithoutNotify((string)parameters[3]);
                _minColorPropertyField.SetValueWithoutNotify((string)parameters[1]);
                _maxColorPropertyField.SetValueWithoutNotify((string)parameters[2]);
                _textureNameField.RegisterValueChangedCallback(e => parameters[0] = e.newValue);
            }
            else
            {
                _textureNameField.SetValueWithoutNotify((string)parameters[0]);
                _colorPropertyField.SetValueWithoutNotify((string)parameters[1]);
                _textureNameField.RegisterValueChangedCallback(e => parameters[0] = e.newValue);
            }
            
            ApplyBindings(useMinMax.value, false);

            useMinMax.RegisterValueChangedCallback(x => ApplyBindings(x.newValue));

            Add(_textureNameField);
            Add(_colorPropertyField);
            Add(controlsList);
            Add(useMinMax);
            Add(_minColorPropertyField);
            Add(_maxColorPropertyField);
        }
        
        private void ApplyBindings(bool value, bool resetArray = true)
        {
            if (value)
            {
                _colorPropertyField.UnregisterValueChangedCallback(SetStringExtraParam1);

                if (resetArray)
                {
                    _parameters.Clear();
                    _parameters.Add(_textureNameField.value);
                    _parameters.Add("");
                    _parameters.Add("");
                    _parameters.Add(_colorPropertyField.value);
                }
                
                _colorPropertyField.RegisterValueChangedCallback(SetStringExtraParam3);
                _minColorPropertyField.RegisterValueChangedCallback(SetStringMinParam1);
                _maxColorPropertyField.RegisterValueChangedCallback(SetStringMaxParam2);
            }
            else
            {
                _colorPropertyField.UnregisterValueChangedCallback(SetStringExtraParam3);
                _minColorPropertyField.UnregisterValueChangedCallback(SetStringMinParam1);
                _maxColorPropertyField.UnregisterValueChangedCallback(SetStringMaxParam2);

                if (resetArray)
                {
                    _parameters.Clear();
                    _parameters.Add(_textureNameField.value);
                    _parameters.Add(_colorPropertyField.value);
                }
                _colorPropertyField.RegisterValueChangedCallback(SetStringExtraParam1);
            }

            _minColorPropertyField.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
            _maxColorPropertyField.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
        }
        
        private void SetStringExtraParam1(ChangeEvent<string> e) => _parameters[1] = e.newValue;
        private void SetStringExtraParam3(ChangeEvent<string> e) => _parameters[3] = e.newValue;
        private void SetStringMinParam1(ChangeEvent<string> e) => _parameters[1] = e.newValue;
        private void SetStringMaxParam2(ChangeEvent<string> e) => _parameters[2] = e.newValue;
    }
}