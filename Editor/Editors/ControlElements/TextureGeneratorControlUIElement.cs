using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

namespace VRLabs.ToonyStandardRebuild
{
    //TODO: finish this shit
    public class TextureGeneratorControlUIElement : VisualElement
    {
        private TextField _textureNameField;
        private TextField _firstExtraField;
        private TextField _secondExtraField;
        private Toggle _customCompute;
        private ObjectField _computeShader;
        private Button _computeSettingsButton;
        private Label _computeSettingsLabelLabel;
        private Label _computeSettingsLabel;
        private string _computeSetting;

        private List<object> _parameters;
        public TextureGeneratorControlUIElement(List<object> Parameters)
        {
            _parameters = Parameters;

            _textureNameField = new TextField("Texture name");
            _firstExtraField = new TextField("First extra property");
            _secondExtraField = new TextField("Second extra property");
            _computeShader = new ObjectField("Compute shader");
            _computeShader.objectType = typeof(ComputeShader);
            _customCompute = new Toggle("Custom compute shader");
            _computeSettingsButton = new Button(SettingsClickAction);
            _computeSettingsButton.text = "Select compute settings JSON";
            _computeSettingsLabelLabel = new Label("Loaded compute settings JSON:");
            _computeSettingsLabelLabel.style.borderTopWidth = 4;
            _computeSettingsLabel = new Label();
            _computeSettingsLabel.AddToClassList("texture-generation-compute-json");
            _customCompute.value = Parameters.Count == 5;

            if (Parameters.Count == 3)
            {
                if (!(Parameters[0] is string))
                    Parameters[0] = "";
                if (!(Parameters[1] is string))
                    Parameters[1] = "";
                if (!(Parameters[2] is string))
                    Parameters[2] = "";
            }
            else if (Parameters.Count == 5)
            {
                if (!(Parameters[0] is ComputeShader))
                    Parameters[0] = null;
                if (!(Parameters[1] is string))
                    Parameters[1] = "";
                if (!(Parameters[2] is string))
                    Parameters[2] = "";
                if (!(Parameters[3] is string))
                    Parameters[3] = "";
                if (!(Parameters[4] is string))
                    Parameters[4] = "";
            }
            else
            {
                Parameters.Clear();
                Parameters.Add("");
                Parameters.Add("");
                Parameters.Add("");
            }

            if (_customCompute.value)
            {
                _computeSetting = (string)Parameters[1];
                _computeShader.SetValueWithoutNotify((Object)Parameters[0]);
                _textureNameField.SetValueWithoutNotify((string)Parameters[2]);
                _firstExtraField.SetValueWithoutNotify((string)Parameters[3]);
                _secondExtraField.SetValueWithoutNotify((string)Parameters[4]);
            }
            else
            {
                _textureNameField.SetValueWithoutNotify((string)Parameters[0]);
                _firstExtraField.SetValueWithoutNotify((string)Parameters[1]);
                _secondExtraField.SetValueWithoutNotify((string)Parameters[2]);
            }

            ApplyBindings(_customCompute.value, false);

            _customCompute.RegisterValueChangedCallback(x => ApplyBindings(x.newValue));

            Add(_textureNameField);
            Add(_firstExtraField);
            Add(_secondExtraField);
            Add(_customCompute);
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

                if (resetArray)
                {
                    _parameters.Clear();
                    _parameters.Add(null);
                    _parameters.Add("");
                    _parameters.Add(_textureNameField.text);
                    _parameters.Add(_firstExtraField.text);
                    _parameters.Add(_secondExtraField.text);
                }

                _computeSettingsLabel.text = _computeSetting;
                _textureNameField.RegisterValueChangedCallback(SetStringParam2);
                _firstExtraField.RegisterValueChangedCallback(SetStringParam3);
                _secondExtraField.RegisterValueChangedCallback(SetStringParam4);
                _computeShader.RegisterValueChangedCallback(SetComputeParam0);
            }
            else
            {
                _textureNameField.UnregisterValueChangedCallback(SetStringParam2);
                _firstExtraField.UnregisterValueChangedCallback(SetStringParam3);
                _secondExtraField.UnregisterValueChangedCallback(SetStringParam4);
                _computeShader.UnregisterValueChangedCallback(SetComputeParam0);

                if (resetArray)
                {
                    _parameters.Clear();
                    _parameters.Add(_textureNameField.text);
                    _parameters.Add(_firstExtraField.text);
                    _parameters.Add(_secondExtraField.text);
                }

                _textureNameField.RegisterValueChangedCallback(SetStringParam0);
                _firstExtraField.RegisterValueChangedCallback(SetStringParam1);
                _secondExtraField.RegisterValueChangedCallback(SetStringParam2);
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
    }
}