using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;

namespace VRLabs.ToonyStandardRebuild.SSIControls
{
    public class ConditionalControlContainerUIElement : VisualElement
    {
        public ConditionalControlContainerUIElement(List<object> parameters, List<ControlUI> controls)
        {
            var parameterNameField = new TextField("Parameter name");
            var enableValueField = new FloatField("Enable value");
            
            var controlsList = new ObjectInspectorList<ControlUI>("Controls", ControlsUIElement.ElementTemplate);
            controlsList.Items = controls;

            if (parameters.Count != 2)
            {
                parameters.Clear();
                parameters.Add("");
                parameters.Add(1f);
            }
            else
            {
                if (!(parameters[0] is string))
                    parameters[0] = "";
                if (!(parameters[1] is float))
                    parameters[1] = 1f;
            }

            parameterNameField.SetValueWithoutNotify((string)parameters[0]);
            enableValueField.SetValueWithoutNotify((float)parameters[1]);
            parameterNameField.RegisterValueChangedCallback(e => parameters[0] = e.newValue);
            enableValueField.RegisterValueChangedCallback(e => parameters[1] = e.newValue);

            Add(parameterNameField);
            Add(enableValueField);
            Add(controlsList);
        }
    }
}