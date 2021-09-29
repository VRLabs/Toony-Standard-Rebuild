using UnityEngine.UIElements;
using System.Collections.Generic;

namespace VRLabs.ToonyStandardRebuild
{
    public class GradientTextureControlUIElement : VisualElement
    {
        public GradientTextureControlUIElement(List<object> parameters)
        {
            var textureNameField = new TextField("Texture name");
            var colorPropertyField = new TextField("Color property");

            if (parameters.Count != 2)
            {
                parameters.Clear();
                parameters.Add("");
                parameters.Add("");
            }
            else
            {
                if (!(parameters[0] is string))
                    parameters[0] = "";
                if (!(parameters[1] is string))
                    parameters[1] = "";
            }

            textureNameField.SetValueWithoutNotify((string)parameters[0]);
            colorPropertyField.SetValueWithoutNotify((string)parameters[1]);
            textureNameField.RegisterValueChangedCallback(e => parameters[0] = e.newValue);
            colorPropertyField.RegisterValueChangedCallback(e => parameters[1] = e.newValue);

            Add(textureNameField);
            Add(colorPropertyField);
        }
    }
}