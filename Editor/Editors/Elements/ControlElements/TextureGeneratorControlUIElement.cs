using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

namespace VRLabs.ToonyStandardRebuild
{
    public class TextureGeneratorControlUIElement : VisualElement
    {
        private TextField _textureNameField;
        private TextField _firstExtraField;
        private TextField _secondExtraField;
        private TextField _uvSetField;
        private ObjectField _computeShader;
        private Button _computeSettingsButton;
        private Label _computeSettingsLabelLabel;
        private Label _computeSettingsLabel;
        private string _computeSetting;

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
            _computeShader = new ObjectField("Compute shader");
            _computeShader.objectType = typeof(ComputeShader);
            var customCompute = new Toggle("Custom compute shader");
            _computeSettingsButton = new Button(SettingsClickAction);
            _computeSettingsButton.text = "Select compute settings JSON";
            _computeSettingsLabelLabel = new Label("Loaded compute settings JSON:");
            _computeSettingsLabelLabel.style.borderTopWidth = 4;
            _computeSettingsLabel = new Label();
            _computeSettingsLabel.AddToClassList("texture-generation-compute-json");
            _computeSettingsLabel.AddToClassList("unity-base-text-field__input");
            _computeSettingsLabel.AddToClassList("unity-text-field__input");
            _computeSettingsLabel.AddToClassList("unity-base-field__input");
            customCompute.value = parameters.Count == 6;

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
            else if (parameters.Count == 6)
            {
                if (!(parameters[0] is ComputeShader))
                    parameters[0] = null;
                if (!(parameters[1] is string))
                    parameters[1] = "";
                if (!(parameters[2] is string))
                    parameters[2] = "";
                if (!(parameters[3] is string))
                    parameters[3] = "";
                if (!(parameters[4] is string))
                    parameters[4] = "";
                if (!(parameters[5] is string))
                    parameters[5] = "";
            }
            else
            {
                parameters.Clear();
                parameters.Add("");
                parameters.Add("");
                parameters.Add("");
                parameters.Add("");
            }

            if (customCompute.value)
            {
                _computeSetting = (string)parameters[1];
                _computeShader.SetValueWithoutNotify((Object)parameters[0]);
                _textureNameField.SetValueWithoutNotify((string)parameters[2]);
                _firstExtraField.SetValueWithoutNotify((string)parameters[3]);
                _secondExtraField.SetValueWithoutNotify((string)parameters[4]);
                _uvSetField.SetValueWithoutNotify((string)parameters[5]);
            }
            else
            {
                _textureNameField.SetValueWithoutNotify((string)parameters[0]);
                _firstExtraField.SetValueWithoutNotify((string)parameters[1]);
                _secondExtraField.SetValueWithoutNotify((string)parameters[2]);
                _uvSetField.SetValueWithoutNotify((string)parameters[3]);
            }

            ApplyBindings(customCompute.value, false);

            customCompute.RegisterValueChangedCallback(x => ApplyBindings(x.newValue));

            Add(_textureNameField);
            Add(_firstExtraField);
            Add(_secondExtraField);
            Add(_uvSetField);
            Add(controlsList);
            Add(customCompute);
            Add(_computeShader);
            Add(_computeSettingsButton);
            Add(_computeSettingsLabelLabel);
            Add(_computeSettingsLabel);
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
                    _parameters.Add("");
                    _parameters.Add(_textureNameField.value);
                    _parameters.Add(_firstExtraField.value);
                    _parameters.Add(_secondExtraField.value);
                    _parameters.Add(_uvSetField.value);
                }

                _computeSettingsLabel.text = _computeSetting;
                _textureNameField.RegisterValueChangedCallback(SetStringParam2);
                _firstExtraField.RegisterValueChangedCallback(SetStringParam3);
                _secondExtraField.RegisterValueChangedCallback(SetStringParam4);
                _uvSetField.RegisterValueChangedCallback(SetStringParam5);
                _computeShader.RegisterValueChangedCallback(SetComputeParam0);
            }
            else
            {
                _textureNameField.UnregisterValueChangedCallback(SetStringParam2);
                _firstExtraField.UnregisterValueChangedCallback(SetStringParam3);
                _secondExtraField.UnregisterValueChangedCallback(SetStringParam4);
                _secondExtraField.UnregisterValueChangedCallback(SetStringParam5);
                _computeShader.UnregisterValueChangedCallback(SetComputeParam0);

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

            _computeShader.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
            _computeSettingsButton.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
            _computeSettingsLabelLabel.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
            _computeSettingsLabel.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void SettingsClickAction()
        {
            string path = EditorUtility.OpenFilePanel("Load compute settings", "Assets", "json");
            if (!File.Exists(path))
            {
                EditorUtility.DisplayDialog("Error", "File does not exist", "Ok");
                return;
            }

            _computeSetting = File.ReadAllText(path);
            _parameters[1] = _computeSetting;
            _computeSettingsLabel.text = _computeSetting;
        }

        private void SetStringParam0(ChangeEvent<string> e) => _parameters[0] = e.newValue;
        private void SetComputeParam0(ChangeEvent<Object> e) => _parameters[0] = e.newValue;
        private void SetStringParam1(ChangeEvent<string> e) => _parameters[1] = e.newValue;
        private void SetStringParam2(ChangeEvent<string> e) => _parameters[2] = e.newValue;
        private void SetStringParam3(ChangeEvent<string> e) => _parameters[3] = e.newValue;
        private void SetStringParam4(ChangeEvent<string> e) => _parameters[4] = e.newValue;
        private void SetStringParam5(ChangeEvent<string> e) => _parameters[5] = e.newValue;
    }
}