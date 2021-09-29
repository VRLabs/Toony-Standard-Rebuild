using UnityEngine.UIElements;
using System.Collections.Generic;

namespace VRLabs.ToonyStandardRebuild
{
    public class VectorControlUIElement : VisualElement
    {
        public VectorControlUIElement(List<object> parameters)
        {
            var parameterNameField = new TextField("Parameter name");
            var showXField = new Toggle("Show X");
            var showYField = new Toggle("Show Y");
            var showZField = new Toggle("Show Z");
            var showAField = new Toggle("Show A");

            if (parameters.Count != 5)
            {
                parameters.Clear();
                parameters.Add("");
                parameters.Add(true);
                parameters.Add(true);
                parameters.Add(true);
                parameters.Add(true);
            }
            else
            {
                if (!(parameters[0] is string))
                    parameters[0] = "";
                if (!(parameters[1] is bool))
                    parameters[1] = true;
                if (!(parameters[2] is bool))
                    parameters[2] = true;
                if (!(parameters[3] is bool))
                    parameters[3] = true;
                if (!(parameters[4] is bool))
                    parameters[4] = true;
            }

            parameterNameField.SetValueWithoutNotify((string)parameters[0]);
            showXField.SetValueWithoutNotify((bool)parameters[1]);
            showYField.SetValueWithoutNotify((bool)parameters[2]);
            showZField.SetValueWithoutNotify((bool)parameters[3]);
            showAField.SetValueWithoutNotify((bool)parameters[4]);
            parameterNameField.RegisterValueChangedCallback(e => parameters[0] = e.newValue);
            showXField.RegisterValueChangedCallback(e => parameters[1] = e.newValue);
            showYField.RegisterValueChangedCallback(e => parameters[2] = e.newValue);
            showZField.RegisterValueChangedCallback(e => parameters[3] = e.newValue);
            showAField.RegisterValueChangedCallback(e => parameters[4] = e.newValue);

            Add(parameterNameField);
            Add(showXField);
            Add(showYField);
            Add(showZField);
            Add(showAField);
        }
    }
}