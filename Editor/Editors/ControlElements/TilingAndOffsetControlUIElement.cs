using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;

namespace VRLabs.ToonyStandardRebuild
{
    public class TilingAndOffsetControlUIElement : VisualElement
    {
        private TextField _textureNameField;
        public TilingAndOffsetControlUIElement(List<object> Parameters)
        {
            _textureNameField = new TextField("Texture name");

            if (Parameters.Count != 1)
            {
                Parameters.Clear();
                Parameters.Add("");
            }
            else
            {
                if (!(Parameters[0] is string))
                    Parameters[0] = "";
            }

            _textureNameField.SetValueWithoutNotify((string)Parameters[0]);
            _textureNameField.RegisterValueChangedCallback(e => Parameters[0] = e.newValue);

            Add(_textureNameField);
        }
    }
}