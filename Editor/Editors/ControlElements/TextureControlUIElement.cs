using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;

namespace VRLabs.ToonyStandardRebuild
{
    public class TextureControlUIElement : VisualElement
    {
        private TextField _textureNameField;
        private TextField _firstExtraField;
        private TextField _secondExtraField;
        public TextureControlUIElement(List<object> Parameters)
        {
            _textureNameField = new TextField("Texture name");
            _firstExtraField = new TextField("First extra property");
            _secondExtraField = new TextField("Second extra property");

            if (Parameters.Count != 3)
            {
                Parameters.Clear();
                Parameters.Add("");
                Parameters.Add("");
                Parameters.Add("");
            }
            else
            {
                if (!(Parameters[0] is string))
                    Parameters[0] = "";
                if (!(Parameters[1] is string))
                    Parameters[1] = "";
                if (!(Parameters[2] is string))
                    Parameters[2] = "";
            }

            _textureNameField.SetValueWithoutNotify((string)Parameters[0]);
            _firstExtraField.SetValueWithoutNotify((string)Parameters[1]);
            _secondExtraField.SetValueWithoutNotify((string)Parameters[2]);
            _textureNameField.RegisterValueChangedCallback(e => Parameters[0] = e.newValue);
            _firstExtraField.RegisterValueChangedCallback(e => Parameters[1] = e.newValue);
            _secondExtraField.RegisterValueChangedCallback(e => Parameters[2] = e.newValue);

            Add(_textureNameField);
            Add(_firstExtraField);
            Add(_secondExtraField);
        }
    }
}