using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;

namespace VRLabs.ToonyStandardRebuild
{
    public class ColorControlUIElement : VisualElement
    {
        private TextField _parameterNameField;
        private Toggle _showAlphaField;
        public ColorControlUIElement(List<object> Parameters)
        {
            _parameterNameField = new TextField("Parameter name");
            _showAlphaField = new Toggle("Show alpha");

            if (Parameters.Count != 2)
            {
                Parameters.Clear();
                Parameters.Add("");
                Parameters.Add(true);
            }
            else
            {
                if (!(Parameters[0] is string))
                    Parameters[0] = "";
                if (!(Parameters[1] is bool))
                    Parameters[1] = true;
            }

            _parameterNameField.SetValueWithoutNotify((string)Parameters[0]);
            _showAlphaField.SetValueWithoutNotify((bool)Parameters[1]);
            _parameterNameField.RegisterValueChangedCallback(e => Parameters[0] = e.newValue);
            _showAlphaField.RegisterValueChangedCallback(e => Parameters[1] = e.newValue);

            Add(_parameterNameField);
            Add(_showAlphaField);
        }
    }
}