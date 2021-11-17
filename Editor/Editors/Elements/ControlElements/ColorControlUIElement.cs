using UnityEngine.UIElements;
using System.Collections.Generic;

namespace VRLabs.ToonyStandardRebuild
{
    public class ColorControlUIElement : VisualElement
    {
        public ColorControlUIElement(List<object> parameters)
        {
            var parameterNameField = new TextField("Parameter name");
            var showAlphaField = new Toggle("Show alpha");

            if (parameters.Count != 2)
            {
                parameters.Clear();
                parameters.Add("");
                parameters.Add(true);
            }
            else
            {
                if (!(parameters[0] is string))
                    parameters[0] = "";
                if (!(parameters[1] is bool))
                    parameters[1] = true;
            }

            parameterNameField.SetValueWithoutNotify((string)parameters[0]);
            showAlphaField.SetValueWithoutNotify((bool)parameters[1]);
            parameterNameField.RegisterValueChangedCallback(e => parameters[0] = e.newValue);
            showAlphaField.RegisterValueChangedCallback(e => parameters[1] = e.newValue);

            Add(parameterNameField);
            Add(showAlphaField);
        }
    }
}