using UnityEngine.UIElements;
using System.Collections.Generic;

namespace VRLabs.ToonyStandardRebuild.SSIControls
{
    public class TilingAndOffsetControlUIElement : VisualElement
    {
        public TilingAndOffsetControlUIElement(List<object> parameters)
        {
            var textureNameField = new TextField("Texture name");

            if (parameters.Count != 1)
            {
                parameters.Clear();
                parameters.Add("");
            }
            else
            {
                if (!(parameters[0] is string))
                    parameters[0] = "";
            }

            textureNameField.SetValueWithoutNotify((string)parameters[0]);
            textureNameField.RegisterValueChangedCallback(e => parameters[0] = e.newValue);

            Add(textureNameField);
        }
    }
}