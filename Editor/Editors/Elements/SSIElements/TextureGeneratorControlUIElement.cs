using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

namespace VRLabs.ToonyStandardRebuild.SSIControls
{
    public class TextureGeneratorControlUIElement : VisualElement
    {
        private TextField _textureNameField;
        private TextField _firstExtraField;
        private TextField _secondExtraField;
        private TextField _uvSetField;
        private ObjectField _shader;

        private List<object> _parameters;
        public TextureGeneratorControlUIElement(List<object> parameters, List<ControlUI> controls)
        {
            _parameters = parameters;

            _textureNameField = new TextField("Texture name");
            _firstExtraField = new TextField("First extra property");
            _secondExtraField = new TextField("Second extra property");
            _uvSetField = new TextField("UV Set ID");
            var controlsList = new ObjectInspectorList<ControlUI>("Settings Controls", ControlsUIElement.ElementTemplate);
            controlsList.Items = controls;
            _shader = new ObjectField("Shader");
            _shader.objectType = typeof(Shader);
            var customShader = new Toggle("Custom Shader");
            customShader.value = parameters.Count == 5;

            if (parameters.Count == 4)
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
            else if (parameters.Count == 5)
            {
                if (!(parameters[0] is Shader))
                    parameters[0] = null;
                if (!(parameters[1] is string))
                    parameters[1] = "";
                if (!(parameters[2] is string))
                    parameters[2] = "";
                if (!(parameters[3] is string))
                    parameters[3] = "";
                if (!(parameters[4] is string))
                    parameters[4] = "";
            }
            else
            {
                parameters.Clear();
                parameters.Add("");
                parameters.Add("");
                parameters.Add("");
                parameters.Add("");
            }

            if (customShader.value)
            {
                _shader.SetValueWithoutNotify((Object)parameters[0]);
                _textureNameField.SetValueWithoutNotify((string)parameters[1]);
                _firstExtraField.SetValueWithoutNotify((string)parameters[2]);
                _secondExtraField.SetValueWithoutNotify((string)parameters[3]);
                _uvSetField.SetValueWithoutNotify((string)parameters[4]);
            }
            else
            {
                _textureNameField.SetValueWithoutNotify((string)parameters[0]);
                _firstExtraField.SetValueWithoutNotify((string)parameters[1]);
                _secondExtraField.SetValueWithoutNotify((string)parameters[2]);
                _uvSetField.SetValueWithoutNotify((string)parameters[3]);
            }

            ApplyBindings(customShader.value, false);

            customShader.RegisterValueChangedCallback(x => ApplyBindings(x.newValue));

            Add(_textureNameField);
            Add(_firstExtraField);
            Add(_secondExtraField);
            Add(_uvSetField);
            Add(controlsList);
            Add(customShader);
            Add(_shader);
        }

        private void ApplyBindings(bool value, bool resetArray = true)
        {
            if (value)
            {
                _textureNameField.UnregisterValueChangedCallback(SetStringParam0);
                _firstExtraField.UnregisterValueChangedCallback(SetStringParam1);
                _secondExtraField.UnregisterValueChangedCallback(SetStringParam2);
                _uvSetField.UnregisterValueChangedCallback(SetStringParam3);

                if (resetArray)
                {
                    _parameters.Clear();
                    _parameters.Add(null);
                    _parameters.Add(_textureNameField.value);
                    _parameters.Add(_firstExtraField.value);
                    _parameters.Add(_secondExtraField.value);
                    _parameters.Add(_uvSetField.value);
                }
                
                _textureNameField.RegisterValueChangedCallback(SetStringParam1);
                _firstExtraField.RegisterValueChangedCallback(SetStringParam2);
                _secondExtraField.RegisterValueChangedCallback(SetStringParam3);
                _uvSetField.RegisterValueChangedCallback(SetStringParam4);
                _shader.RegisterValueChangedCallback(SetShaderParam0);
            }
            else
            {
                _textureNameField.UnregisterValueChangedCallback(SetStringParam1);
                _firstExtraField.UnregisterValueChangedCallback(SetStringParam2);
                _secondExtraField.UnregisterValueChangedCallback(SetStringParam3);
                _secondExtraField.UnregisterValueChangedCallback(SetStringParam4);
                _shader.UnregisterValueChangedCallback(SetShaderParam0);

                if (resetArray)
                {
                    _parameters.Clear();
                    _parameters.Add(_textureNameField.value);
                    _parameters.Add(_firstExtraField.value);
                    _parameters.Add(_secondExtraField.value);
                    _parameters.Add(_uvSetField.value);
                }

                _textureNameField.RegisterValueChangedCallback(SetStringParam0);
                _firstExtraField.RegisterValueChangedCallback(SetStringParam1);
                _secondExtraField.RegisterValueChangedCallback(SetStringParam2);
                _uvSetField.RegisterValueChangedCallback(SetStringParam3);
            }

            _shader.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void SetStringParam0(ChangeEvent<string> e) => _parameters[0] = e.newValue;
        private void SetShaderParam0(ChangeEvent<Object> e) => _parameters[0] = e.newValue;
        private void SetStringParam1(ChangeEvent<string> e) => _parameters[1] = e.newValue;
        private void SetStringParam2(ChangeEvent<string> e) => _parameters[2] = e.newValue;
        private void SetStringParam3(ChangeEvent<string> e) => _parameters[3] = e.newValue;
        private void SetStringParam4(ChangeEvent<string> e) => _parameters[4] = e.newValue;
    }
}