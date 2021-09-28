using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;

namespace VRLabs.ToonyStandardRebuild
{
    public class VectorControlUIElement : VisualElement
    {
        private TextField _parameterNameField;
        private Toggle _showXField;
        private Toggle _showYField;
        private Toggle _showZField;
        private Toggle _showAField;
        public VectorControlUIElement(List<object> Parameters)
        {
            _parameterNameField = new TextField("Parameter name");
            _showXField = new Toggle("Show X");
            _showYField = new Toggle("Show Y");
            _showZField = new Toggle("Show Z");
            _showAField = new Toggle("Show A");

            if (Parameters.Count != 5)
            {
                Parameters.Clear();
                Parameters.Add("");
                Parameters.Add(true);
                Parameters.Add(true);
                Parameters.Add(true);
                Parameters.Add(true);
            }
            else
            {
                if (!(Parameters[0] is string))
                    Parameters[0] = "";
                if (!(Parameters[1] is bool))
                    Parameters[1] = true;
                if (!(Parameters[2] is bool))
                    Parameters[2] = true;
                if (!(Parameters[3] is bool))
                    Parameters[3] = true;
                if (!(Parameters[4] is bool))
                    Parameters[4] = true;
            }

            _parameterNameField.SetValueWithoutNotify((string)Parameters[0]);
            _showXField.SetValueWithoutNotify((bool)Parameters[1]);
            _showYField.SetValueWithoutNotify((bool)Parameters[2]);
            _showZField.SetValueWithoutNotify((bool)Parameters[3]);
            _showAField.SetValueWithoutNotify((bool)Parameters[4]);
            _parameterNameField.RegisterValueChangedCallback(e => Parameters[0] = e.newValue);
            _showXField.RegisterValueChangedCallback(e => Parameters[1] = e.newValue);
            _showYField.RegisterValueChangedCallback(e => Parameters[2] = e.newValue);
            _showZField.RegisterValueChangedCallback(e => Parameters[3] = e.newValue);
            _showAField.RegisterValueChangedCallback(e => Parameters[4] = e.newValue);

            Add(_parameterNameField);
            Add(_showXField);
            Add(_showYField);
            Add(_showZField);
            Add(_showAField);
        }
    }
}