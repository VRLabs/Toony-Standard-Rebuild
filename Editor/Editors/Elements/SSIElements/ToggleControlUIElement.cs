using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;

namespace VRLabs.ToonyStandardRebuild.SSIControls
{
    public class ToggleControlUIElement : VisualElement
    {
        public ToggleControlUIElement(List<object> parameters)
        {
            var togglePropertyField = new TextField("Property name");
            var falseValueField = new FloatField("False value");
            var trueValueField = new FloatField("True value");

            if (parameters.Count != 3)
            {
                parameters.Clear();
                parameters.Add("");
                parameters.Add(0f);
                parameters.Add(1f);
            }
            else
            {
                if (!(parameters[0] is string))
                    parameters[0] = "";
                if (!(parameters[1] is float))
                    parameters[1] = 0f;
                if (!(parameters[2] is float))
                    parameters[2] = 1f;
            }

            togglePropertyField.SetValueWithoutNotify((string)parameters[0]);
            falseValueField.SetValueWithoutNotify((float)parameters[1]);
            trueValueField.SetValueWithoutNotify((float)parameters[2]);
            togglePropertyField.RegisterValueChangedCallback(e => parameters[0] = e.newValue);
            falseValueField.RegisterValueChangedCallback(e => parameters[1] = e.newValue);
            trueValueField.RegisterValueChangedCallback(e => parameters[2] = e.newValue);

            Add(togglePropertyField);
            Add(falseValueField);
            Add(trueValueField);
        }
    }
}