using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;

namespace VRLabs.ToonyStandardRebuild
{
    public class GradientTextureControlUIElement : VisualElement
    {
        private TextField _textureNameField;
        private TextField _colorPropertyField;
        public GradientTextureControlUIElement(List<object> Parameters)
        {
            _textureNameField = new TextField("Texture name");
            _colorPropertyField = new TextField("Color property");

            if (Parameters.Count != 2)
            {
                Parameters.Clear();
                Parameters.Add("");
                Parameters.Add("");
            }
            else
            {
                if (!(Parameters[0] is string))
                    Parameters[0] = "";
                if (!(Parameters[1] is string))
                    Parameters[1] = "";
            }

            _textureNameField.SetValueWithoutNotify((string)Parameters[0]);
            _colorPropertyField.SetValueWithoutNotify((string)Parameters[1]);
            _textureNameField.RegisterValueChangedCallback(e => Parameters[0] = e.newValue);
            _colorPropertyField.RegisterValueChangedCallback(e => Parameters[1] = e.newValue);

            Add(_textureNameField);
            Add(_colorPropertyField);
        }
    }
}