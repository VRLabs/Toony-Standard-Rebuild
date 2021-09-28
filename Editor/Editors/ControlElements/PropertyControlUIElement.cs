using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;

namespace VRLabs.ToonyStandardRebuild
{
    public class PropetyControlUIElement : VisualElement
    {
        private TextField _parameterNameField;
        public PropetyControlUIElement(List<object> Parameters)
        {
            _parameterNameField = new TextField("Parameter name");

            if (Parameters.Count != 1)
            {
                Parameters.Clear();
                Parameters.Add("");
            }
            else if (!(Parameters[0] is string))
            {
                Parameters[0] = "";
            }

            _parameterNameField.SetValueWithoutNotify((string)Parameters[0]);
            _parameterNameField.RegisterValueChangedCallback(e => Parameters[0] = e.newValue);

            Add(_parameterNameField);
        }
    }
}