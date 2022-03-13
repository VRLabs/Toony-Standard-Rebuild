using UnityEngine.UIElements;
using System.Collections.Generic;

namespace VRLabs.ToonyStandardRebuild.SSIControls
{
    public class TextureControlUIElement : VisualElement
    {
        public TextureControlUIElement(List<object> parameters, List<ControlUI> controls)
        {
            var textureNameField = new TextField("Texture name");
            var firstExtraField = new TextField("First extra property");
            var secondExtraField = new TextField("Second extra property");
            var uvSetField = new TextField("UV Set ID");
            var controlsList = new ObjectInspectorList<ControlUI>("Settings Controls", ControlsUIElement.ElementTemplate);
            controlsList.Items = controls;

            if (parameters.Count != 4)
            {
                parameters.Clear();
                parameters.Add("");
                parameters.Add("");
                parameters.Add("");
                parameters.Add("");
            }
            else
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

            textureNameField.SetValueWithoutNotify((string)parameters[0]);
            firstExtraField.SetValueWithoutNotify((string)parameters[1]);
            secondExtraField.SetValueWithoutNotify((string)parameters[2]);
            uvSetField.SetValueWithoutNotify((string)parameters[3]);
            textureNameField.RegisterValueChangedCallback(e => parameters[0] = e.newValue);
            firstExtraField.RegisterValueChangedCallback(e => parameters[1] = e.newValue);
            secondExtraField.RegisterValueChangedCallback(e => parameters[2] = e.newValue);
            uvSetField.RegisterValueChangedCallback(e => parameters[3] = e.newValue);

            Add(textureNameField);
            Add(firstExtraField);
            Add(secondExtraField);
            Add(uvSetField);
            Add(controlsList);
        }
    }
}