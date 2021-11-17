using UnityEngine.UIElements;
using System.Collections.Generic;

namespace VRLabs.ToonyStandardRebuild
{
    public class PropetyControlUIElement : VisualElement
    {
        public PropetyControlUIElement(List<object> parameters)
        {
            var parameterNameField = new TextField("Parameter name");

            if (parameters.Count != 1)
            {
                parameters.Clear();
                parameters.Add("");
            }
            else if (!(parameters[0] is string))
            {
                parameters[0] = "";
            }

            parameterNameField.SetValueWithoutNotify((string)parameters[0]);
            parameterNameField.RegisterValueChangedCallback(e => parameters[0] = e.newValue);

            Add(parameterNameField);
        }
    }
}